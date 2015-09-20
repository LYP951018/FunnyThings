#pragma once

#include <d2d1.h>
#include <d2d1helper.h>
#include <dwrite.h>
#include "ComPtr.h"

namespace Yupei
{
	extern ComPtr<ID2D1Factory> g_D2DFactory;
	extern ComPtr<IDWriteFactory> g_DWriteFactory;

	void InitializeFactories();


}
