#include "Win32Wrappers.hpp"
#include <Windows.h>

namespace funny
{
    int CloseHandleWrapper(NativeWindowHandle handle) noexcept
    {
        return ::CloseHandle(handle);
    }

}
