#include "PEFile.hpp"
#include <Assert.hpp>
#include <OS/Windows/Win32Exception.hpp>
#include <Windows.h>

void PEFile::MapFile()
{
    YPASSERT(fileHandle_ != Yupei::GetInvalidWindowHandle(), "Invalid file handle!");
    mappingHandle_.Reset(::CreateFileMapping(fileHandle_, {}, PAGE_READONLY, {}, {}, {}));
    if (mappingHandle_)
    {
        basicAddr_ = static_cast<const Yupei::byte*>(::MapViewOfFile(mappingHandle_.Get(), FILE_MAP_READ, {}, {}, {}));
        if (basicAddr_ == nullptr)
        {
			THROW_WIN32_EXCEPTION;
        }
    }
}
