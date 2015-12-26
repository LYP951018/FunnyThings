#pragma once

#include "Basic.h"
#include "SlidingTextWindow.h"
#include "SyntaxDiagnosticInfo.h"

#include <vector>

namespace funny
{
	class AbstractLexer : public Noncopymoveable
	{

	protected:
		SlidingTextWindow textWindow_;

		//TODO: StringType or other source?
		AbstractLexer(const StringType& text)
			:textWindow_{text}
		{}

		const std::vector<SyntaxDiagnosticInfo>& GetErrors() const noexcept
		{
			return errors_;
		}

		//TODO: private or protected?
	private:
		std::vector<SyntaxDiagnosticInfo> errors_;
	};
}
