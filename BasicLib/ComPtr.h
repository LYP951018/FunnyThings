#pragma once

#include <type_traits> // for is_convertible
#include <utility>
#include "Assert.h"

namespace funny
{
	template<typename Interface>
	class ComPtr
	{
	static_assert(!std::is_final<Interface>::value, "No final class support.");
	private:
		template<typename OtherInterface>
		class RemoveAddRefRelease : public OtherInterface
		{
		private:
			unsigned long __stdcall AddRef();
			unsigned long __stdcall Release();
		};
	public:

		template<typename OtherInterface>
		friend class ComPtr;

		constexpr ComPtr() noexcept = default;

		template<
			typename OtherInterface,
			typename = std::enable_if_t<std::is_convertible<OtherInterface*, Interface*>::value >>
		explicit ComPtr(OtherInterface* p)
			:ptr_(p)
		{
			if (p) p->AddRef();
		}

		constexpr ComPtr(nullptr_t) noexcept : ComPtr() {}

		ComPtr(const ComPtr& rhs) noexcept
			:ptr_(rhs.ptr_)
		{
			InternalAddRef();
		}

		template<
			typename OtherInterface,
			typename = std::enable_if_t<std::is_convertible<OtherInterface*,Interface*>::value>>
		ComPtr(const ComPtr<OtherInterface>& rhs) noexcept
		{
			InternalAddRef();
		}

		ComPtr(ComPtr&& rhs) noexcept
			:ptr_(rhs.ptr_)
		{
			rhs.ptr_ = nullptr;
		}

		template<
			typename OtherInterface,
			typename = std::enable_if_t<std::is_convertible<OtherInterface*, Interface*>::value >>
			ComPtr(ComPtr<OtherInterface>&& rhs) noexcept
		{
			rhs.ptr_ = nullptr;
		}

		ComPtr& operator = (const ComPtr& rhs) noexcept
		{
			if (ptr_ != rhs.ptr_)
				ComPtr(rhs).swap(*this);
			return *this;
		}

		template<
			typename OtherInterface,
			typename = std::enable_if_t<std::is_assignable<Interface*,OtherInterface*>::value>>
		ComPtr& operator = (const ComPtr<OtherInterface>& rhs) noexcept
		{
			if(ptr_ != rhs.ptr_)	
				ComPtr(rhs).swap(*this);
			return *this;
		}

		ComPtr& operator = (ComPtr&& rhs) noexcept
		{
			if (ptr_ != rhs.ptr_)
				ComPtr(std::move(rhs)).swap(*this);
			return *this;
		}

		template<
			typename OtherInterface,
			typename = std::enable_if_t<std::is_assignable<Interface*, OtherInterface*>::value >>
			ComPtr& operator = (ComPtr<OtherInterface>&& rhs) noexcept
		{
			if (ptr_ != rhs.ptr_)
				ComPtr(std::move(rhs)).swap(*this);
			return *this;
		}

		~ComPtr() noexcept
		{
			InternalRelease();
		}

		void swap(ComPtr& rhs)
		{
			std::swap(ptr_, rhs.ptr_);
		}

		RemoveAddRefRelease<Interface>* operator->() const noexcept
		{
			return static_cast<RemoveAddRefRelease<Interface>*>(ptr_);
		}

		explicit operator bool() const noexcept
		{
			return ptr_ != nullptr;
		}

		void Reset() noexcept
		{
			ComPtr().swap(*this);
		}

		Interface* Get() const noexcept
		{
			return ptr_;
		}

		Interface** GetAddressOf() noexcept
		{
			YPASSERT(ptr_ != nullptr, "GetAddressOf can't be called on a null ptr_");
			return &ptr_;
		}

		void Copy(Interface* p)
		{
			ComPtr(p).swap(*this);
		}

		void Attach(Interface* p)
		{
			InternalRelease();
			ptr_ = p;
		}

		Interface* Detach() noexcept
		{
			auto tmp = ptr_;
			ptr_ = nullptr;
			return tmp;
		}

		void CopyTo(Interface** pp)
		{
			InternalAddRef();
			*pp = ptr_;
		}

		template<typename T>
		ComPtr<T> As() const noexcept
		{
			ComPtr<T> temp;
			ptr_->QueryInterface(temp.GetAddressOf());
			return temp;
		}
	private:
		Interface* ptr_ = nullptr;

		void InternalAddRef() const noexcept
		{
			if (ptr_) ptr_->AddRef();
		}

		void InternalRelease() noexcept
		{
			auto tmp = ptr_;
			if (tmp)
			{
				ptr_ = nullptr;
				tmp->Release();
			}
		}
	};

	template<typename Interface>
	void swap(
		ComPtr<Interface>& lhs,
		ComPtr<Interface>& rhs)
	{
		lhs.swap(rhs);
	}
}
