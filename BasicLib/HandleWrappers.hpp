#pragma once

#include "HandleWrapper.hpp"
#include "Win32Wrappers.hpp"

namespace funny
{
    struct FileHandleCloser
    {
        using HandleType = NativeWindowHandle;
        static constexpr HandleType InvalidHandle = InvalidWindowHandleValue;

        void operator()(NativeWindowHandle handle) noexcept
        {
            (void)CloseHandleWrapper(handle);
        }
    };

    extern template class HandleWrapper<FileHandleCloser>;

    using FileHandle = HandleWrapper<FileHandleCloser>;

    struct NormalHandleCloser
    {
        using HandleType = NativeWindowHandle;
        static constexpr HandleType InvalidHandle = 0;

        void operator()(NativeWindowHandle handle) noexcept
        {
            (void)CloseHandleWrapper(handle);
        }
    };

    extern template class HandleWrapper<NormalHandleCloser>;

    using MappingFileHandle = HandleWrapper<NormalHandleCloser>;
}