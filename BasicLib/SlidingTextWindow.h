#pragma once

#include "StringType.h"
#include <limits>


class SlidingTextWindow
{

public:
	static constexpr CharType InvalidCharacter = std::numeric_limits<CharType>::max();

	SlidingTextWindow() noexcept = default;

	SlidingTextWindow(StringType str);

	void AdvanceChar() noexcept
	{
		AdvanceChar(1);
	}

	void AdvanceChar(std::size_t n) noexcept;

	CharType PeekChar() noexcept
	{
		if (pos_ >= lineContent_.length()) return InvalidCharacter;
		return lineContent_[pos_];
	}

	CharType PeekChar(std::size_t delta) noexcept;

	CharType NextChar() noexcept;

	bool AdvanceIfPositiveInteger(int& result) noexcept;

	bool AdvanceIfMatches(CharType c) noexcept;

	bool AdvanceIfMatches(const StringType& str) noexcept;

public:

	StringType lineContent_;
	std::size_t pos_{};

	void Reset(std::size_t pos) noexcept;
};

