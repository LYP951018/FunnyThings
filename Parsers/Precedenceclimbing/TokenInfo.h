#pragma once

#include "SyntaxKind.h"
#include "StringType.h"
#include "Basic.h"


struct TokenInfo
{

	SyntaxKind Kind_;
	PrimitiveType Type_;


	funny::StringType Text_;
	union
	{
		funny::int32 Int32Value_;
		funny::uint32 UInt32Value_;
		/*int64 Int64Value_;
		uint64 UInt64Value_;*/
		/*float FloatValue_;*/
		double DoubleValue_;
	};
};

