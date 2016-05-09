#pragma once

#include <cstdint>

enum class ImgaeMachineType : std::uint32_t
{
    kI386 = 0x014c,      //x86
    kIA64 = 0x0200,      //Intel Itanium
    kAmd64 = 0x8664      //x64
};

enum class ImageFileHeaderCharacteristic : std::uint32_t
{
    kNoReallocations = 0x0001,
    kExecutable = 0x0002,
    kNoLineNumbers = 0x0004,
    kNoSymbolTables = 0x0008,
    kTrimWorkingSet = 0x0010,
    kLargeAddressAware = 0x0020,
    kBytesReversedLo = 0x0080,
    kSupport32bitMachine = 0x0100,
    kNoDebugInfo = 0x0020,
    kRemoveableRunOnSwap = 0x0800,
    kSystemFile = 0x1000,
    kDllFile = 0x2000,
    kUniprocessorOnly = 0x4000,
    kBytesReversedHi = 0x8000
};

struct ImageFileHeader
{
    ImgaeMachineType Machine_;
    std::uint8_t SectionNum_;                           //Note that the Windows loader limits the number of sections to 96.
    std::uint32_t CreatedTime_;                         //The low 32 bits of the time stamp of the image.
    std::uint32_t SymbolTableOffset_;                   //The offset of the symbol table, in bytes, or zero if no COFF symbol table exists.
    std::uint32_t SymbolNum_;                           //The number of symbols in the symbol table.
    std::uint8_t OptionalHeaderSize_;                   //The size of the optional header, in bytes. This value should be 0 for object files.
    ImageFileHeaderCharacteristic Characteristics_;
};