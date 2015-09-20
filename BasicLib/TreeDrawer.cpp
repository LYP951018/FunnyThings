#include "TreeDrawer.h"

namespace Yupei
{
	static ComPtr<ID2D1Factory> g_D2DFactory;
	static ComPtr<IDWriteFactory> g_DWriteFactory;

	void InitializeFactories()
	{
		D2D1CreateFactory(
			D2D1_FACTORY_TYPE_SINGLE_THREADED,
			g_D2DFactory.GetAddressOf());
		DWriteCreateFactory(
			DWRITE_FACTORY_TYPE_SHARED,
			__uuidof(IDWriteFactory),
			reinterpret_cast<IUnknown**>(g_DWriteFactory.GetAddressOf()));
	}

}
