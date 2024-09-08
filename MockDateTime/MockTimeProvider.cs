using System.Reflection;
using System.Reflection.Emit;

namespace MockDateTime;

public class MockTimeProvider
{
    public static void Init()
    {
        IntPtr timeFuncPtr = GetTimeFuncPtr();
        if (IntPtr.Zero == timeFuncPtr)
        {
            throw new InvalidOperationException("TimeFunc not found");
        }
        
        FieldInfo? foundFieldInfo = null;
        
#if NET8_0
        Type? leapClass = typeof(DateTime).GetNestedType("LeapSecondCache", BindingFlags.NonPublic);
        if (null != leapClass)
        {
            foreach (FieldInfo fieldInfo in leapClass.GetRuntimeFields())
            {
                if (fieldInfo.Name == "s_pfnGetSystemTimeAsFileTime" || fieldInfo.FieldType == typeof(IntPtr))
                {
                    foundFieldInfo = fieldInfo;
                    break;
                }
            }
        }
#else
        foreach (FieldInfo fieldInfo in typeof(DateTime).GetRuntimeFields())
        {
            if (fieldInfo.Name == "s_pfnGetSystemTimeAsFileTime" || fieldInfo.FieldType == typeof(IntPtr))
            {
                foundFieldInfo = fieldInfo;
                break;
            }
        }
#endif
        
        if (null == foundFieldInfo)
        {
            throw new InvalidOperationException("DateTime GetSystemTimeAsFileTime Field not found");
        }

        DynamicMethod hookMethod = new DynamicMethod(
            name: "HookGetSystemTimeAsFileTime",
            returnType: null,
            parameterTypes: new []{ typeof(IntPtr) },
            restrictedSkipVisibility: true
        );

        ILGenerator ilGenerator = hookMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Stsfld, foundFieldInfo);
        ilGenerator.Emit(OpCodes.Ret);
        
        Action<IntPtr> hookAction = (Action<IntPtr>)hookMethod.CreateDelegate(typeof(Action<IntPtr>));
        hookAction(timeFuncPtr);
    }
    
    private static IntPtr GetTimeFuncPtr()
    {
        MethodInfo? methodInfo = typeof(MockTimeProvider).GetMethod(nameof(TimeFunc), BindingFlags.NonPublic | BindingFlags.Static);
        return methodInfo?.MethodHandle.GetFunctionPointer() ?? IntPtr.Zero;
    }
    
    private static unsafe void TimeFunc(ulong* ptr)
    {
        NativeMethods.GetSystemTimeAsFileTime(out NativeMethods.FileTime fileTime);
        ulong fileTime64 = ((ulong)fileTime.dwHighDateTime << 32) | fileTime.dwLowDateTime;
        // 1 day add
        fileTime64 += 864000000000;
        *ptr = fileTime64;
    }
}