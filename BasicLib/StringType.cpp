#include "StringType.h"
#include <iostream>

#if defined(YPUTF16)

std::basic_istream<CharType>& CIn = std::wcin;
std::basic_ostream<CharType>& COut = std::wcout;

#elif defined(YPUTF8)

std::basic_istream<CharType>& CIn = std::cin;
std::basic_ostream<CharType>& COut = std::cout;

#endif