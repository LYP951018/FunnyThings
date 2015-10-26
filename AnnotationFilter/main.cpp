#include <string>
#include <iostream>
#include <cassert>
#include <exception>
#include <iterator>
#include <fstream>

using StringType = std::string;

using namespace std::string_literals;

constexpr char InvalidCharacter = CHAR_MAX;


class SyntaxError : public std::logic_error
{
public:
	SyntaxError(const char* str) : logic_error(str) {}
};

/*
 A simple scanner
*/
struct SourceCode
{
	std::size_t pos_;
	const StringType sourceCode_;

	SourceCode(StringType str)
		:pos_{},
		sourceCode_{ std::move(str) }
	{
	}

	bool AdvanceIfMatch(char c) noexcept
	{
		if (PeekChar() == c)
		{
			AdvanceChar();
			return true;
		}
		return false;
	}

	char NextChar() noexcept
	{
		auto c = PeekChar();
		if (c != InvalidCharacter)
			AdvanceChar();
		return c;
	}

	bool AdvanceIfMatch(const StringType & str) noexcept
	{
		auto len = str.length();
		for (std::size_t i = 0;i < len;++i)
			if (PeekChar(i) != str[i]) return false;
		AdvanceChar(len);
		return true;
	}

	char PeekChar() noexcept
	{
		if (pos_ >= sourceCode_.length()) return InvalidCharacter;
		return sourceCode_[pos_];
	}

	char PeekChar(std::size_t delta) noexcept // /*
	{
		auto tmp = pos_;
		AdvanceChar(delta);
		char ret;
		if (pos_ >= sourceCode_.length())
			ret = InvalidCharacter;
		else
			ret = sourceCode_[pos_];
		Reset(tmp);
		return ret;
	}

	void Reset(std::size_t pos) noexcept
	{
		assert(pos <= sourceCode_.length());
		pos_ = pos;
	}

	void AdvanceChar(std::size_t n) noexcept
	{
		pos_ += n;
		assert(pos_ <= sourceCode_.length() && pos_ >= 0);
	}

	void AdvanceChar() noexcept { AdvanceChar(1); }

	bool IsPosValid() const noexcept { return pos_ <= sourceCode_.length(); }

};

StringType OnLiterals(SourceCode& sourceCode,char endChar)
{
	StringType res;
	res += endChar;
	while (true)
	{
		if (sourceCode.AdvanceIfMatch('\\'))
		{
			res += '\\';
			res += sourceCode.NextChar();			
		}
		else if (sourceCode.AdvanceIfMatch(endChar))
		{
			res += endChar;
			return std::move(res);
		}
		else
		{
			auto c = sourceCode.NextChar();
			if (c == InvalidCharacter)
				break;			
			res += c;
		}
	}
	std::string exp;
	(exp += endChar) += " lost";
	throw SyntaxError(exp.c_str());
}

void OnMultiLine(SourceCode& sourceCode)
{
	while (sourceCode.IsPosValid())
		if (sourceCode.AdvanceIfMatch("*/"s))
			return;
		else
			sourceCode.AdvanceChar();
	throw SyntaxError("*/ lost");
}

void OnSingleLine(SourceCode& sourceCode)
{
	while (sourceCode.IsPosValid())
		if (sourceCode.PeekChar() == '\n')
			return;
		else
			sourceCode.AdvanceChar();		
}

StringType Filter(SourceCode& sourceCode)
{

	StringType res;
	while (true)
	{
		if (sourceCode.AdvanceIfMatch('\"'))
		{
			res += OnLiterals(sourceCode,'\"');
		}
		else if (sourceCode.AdvanceIfMatch('\''))
		{
			res += OnLiterals(sourceCode,'\'');
		}
		else if (sourceCode.AdvanceIfMatch("/*"s))
		{
			OnMultiLine(sourceCode);
		}
		else if (sourceCode.AdvanceIfMatch("//"s))
		{
			OnSingleLine(sourceCode);
		}
		else
		{
			auto c = sourceCode.NextChar();
			if (c == InvalidCharacter)
				break;
			res += c;
		}
	}
	return res;
}

int main(int argc, char* argv[])
{
	if (argc == 1) return 0;
	else
	{
		std::ifstream input{ argv[1] };
		if (input)
		{
			std::string str{
				std::istreambuf_iterator<char>{input},
				std::istreambuf_iterator<char>{} };
			//std::cout << str;
			SourceCode code{ std::move(str) };
			std::ofstream output{ ".\\result.txt" };
			if(output)
				output << Filter(code);
			//std::cout << Filter(code);
		}
	}
	std::getchar();
	return 0;
}