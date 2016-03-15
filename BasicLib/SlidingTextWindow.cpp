#include "SlidingTextWindow.hpp"

#include <utility>
#include <cassert>

namespace funny
{
	SlidingTextWindow::SlidingTextWindow(StringType str)
		:lineContent_{ std::move(str) }
	{

	}

	void SlidingTextWindow::AdvanceChar(std::size_t n) noexcept
	{
		pos_ += n;
		assert(pos_ <= lineContent_.length() && pos_ >= 0);
	}

	bool SlidingTextWindow::AdvanceIfMatches(const StringType & str) noexcept
	{
		auto len = str.length();
		for (std::size_t i = 0;i < len;++i)
			if (PeekChar(i) != str[i]) return false;
		AdvanceChar(len);
		return true;
	}

	void SlidingTextWindow::Reset(std::size_t pos) noexcept
	{
		assert(pos <= lineContent_.length());
		pos_ = pos;
	}

	CharType SlidingTextWindow::PeekChar(std::size_t delta) noexcept
	{
		auto tmp = pos_;
		AdvanceChar(delta);
		CharType ret;
		if (pos_ >= lineContent_.length())
			ret = InvalidCharacter;
		else
			ret = lineContent_[pos_];
		Reset(tmp);
		return ret;
	}

	CharType SlidingTextWindow::NextChar() noexcept
	{
		auto c = PeekChar();
		if (c != InvalidCharacter)
			AdvanceChar();
		return c;
	}

	bool SlidingTextWindow::AdvanceIfPositiveInteger(int & result) noexcept
	{
		CharType c;
		std::size_t i{};
		int res{};
		c = PeekChar(i);
		while (CharUtility::IsDigit(c))
		{
			res = 10 * res + CharUtility::ToInt(c);
			c = PeekChar(++i);
		}
		AdvanceChar(i);
		result = res;
		return i != 0;
	}

	bool SlidingTextWindow::AdvanceIfMatches(CharType c) noexcept
	{
		if (PeekChar() == c)
		{
			AdvanceChar();
			return true;
		}
		return false;
	}

}


