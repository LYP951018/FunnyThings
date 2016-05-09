#pragma once

#include <Basic.hpp>
#include <OS\Windows\WinDef.hpp>
#include <OS\Windows\HandleWrappers.hpp>

class PEFile
{
public:
    PEFile() noexcept
        :fileHandle_{Yupei::GetInvalidWindowHandle()}
    {}

    PEFile(Yupei::NativeWindowHandle fileHandle) noexcept
        :fileHandle_{fileHandle}
    {}

private:
    void MapFile();

	Yupei::NativeWindowHandle fileHandle_;
	Yupei::FileHandle mappingHandle_;
    const Yupei::byte* basicAddr_;
};