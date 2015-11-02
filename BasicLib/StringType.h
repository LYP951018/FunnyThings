#pragma once

#pragma once

//provides a unifed interface for UTF-16 based platform (such as Windows) and UTF-8 based plateform (such as *nix).

#include <iosfwd>
#include <string> //for std::basic_string

#ifdef _WIN32

using CharType = wchar_t;

#define YPUTF16 1

#ifndef TEXT
#define TEXT(S) L##S
#endif

#elif __unix || __unix__ || __APPLE__ || __MACH__ || __linux__ || __FreeBSD__

using CharType = char;

#define YPUTF8 1

#ifndef TEXT
#define TEXT(S) S
#endif


#else

#error Unsupported platform.

#endif

using StringType = std::basic_string<CharType>;
using InStreamType = std::basic_istream<CharType>;
using FileInStreamType = std::basic_ifstream<CharType>;
extern std::basic_istream<CharType>& CIn;
extern std::basic_ostream<CharType>& COut;


class CharUtility
{
public:
	template<typename IntT>
	static bool IsDigit(IntT c) noexcept
	{
		return c >= TEXT('0') && c <= TEXT('9');
	}

	template<typename IntT>
	static bool IsAlpha(IntT c) noexcept
	{
		return (c >= TEXT('a') && c <= TEXT('z'))
			|| (c >= TEXT('A') && c <= TEXT('Z'));
	}

	template<typename CharT>
	static int ToInt(CharT c) noexcept
	{
		return c - TEXT('0');
	}

	template<typename CharT>
	static bool IsSpace(CharT c) noexcept
	{
		return c == TEXT(' ') ||
			c == TEXT('\t') ||
			c == TEXT('\r') ||
			c == TEXT('\n');
	}

};



