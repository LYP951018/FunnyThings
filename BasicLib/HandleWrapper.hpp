#pragma once

#include "Nonable.hpp"
#include <utility>

namespace funny
{
    template<typename Closer>
    class HandleWrapper : Noncopyable
    {
    public:
        using HandleType = typename Closer::HandleType;

        static constexpr HandleType InvalidHandle = Closer::InvalidHandle;

        constexpr HandleWrapper() noexcept
            :handle_{InvalidHandle}
        {}

        explicit constexpr HandleWrapper(HandleType handle) noexcept
            :handle_{handle}
        {}

        constexpr HandleWrapper(HandleWrapper&& other) noexcept
            :handle_ {other.handle_}
        {
            other.handle_ = InvalidHandle;
        }

        HandleType& operator=(HandleWrapper&& other) noexcept
        {
            HandleWrapper {std::move(i)}.swap(*this);
            return *this;
        }

        void swap(HandleWrapper& other) noexcept
        {
            std::swap(handle_, other.handle_);
        }

        explicit operator bool() const noexcept
        {
            return handle_ != InvalidHandle;
        }

        HandleType Get() const noexcept
        {
            return handle_;
        }

        void Reset(HandleType handle) noexcept
        {
            handle_ = handle;
        }

    private:
        HandleType handle_;
    };

    template<typename T>
    void swap(HandleWrapper<T>& lhs, HandleWrapper<T>& rhs)
    {
        lhs.swap(rhs);
    }
}