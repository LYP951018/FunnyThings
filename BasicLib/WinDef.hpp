#pragma once

#include <cstdint>

namespace funny
{
    //https://msdn.microsoft.com/en-us/library/windows/desktop/aa383751(v=vs.85).aspx

    using NativeWindowHandle = void*;   
    static constexpr NativeWindowHandle InvalidWindowHandleValue = reinterpret_cast<NativeWindowHandle>(static_cast<std::intptr_t>(-1));

    //TODO: undef GetMessage stuff.
}