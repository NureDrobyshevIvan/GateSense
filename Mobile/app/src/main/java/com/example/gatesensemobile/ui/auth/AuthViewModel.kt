package com.example.gatesensemobile.ui.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gatesensemobile.data.api.RegisterRequest
import com.example.gatesensemobile.data.api.userMessage
import com.example.gatesensemobile.data.repository.AuthRepository
import com.example.gatesensemobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class AuthUiState(
    val loading: Boolean = false,
    val error: String? = null,
    val success: Boolean = false
)

class AuthViewModel(
    private val repo: AuthRepository = ServiceLocator.authRepository
) : ViewModel() {

    private val _state = MutableStateFlow(AuthUiState())
    val state: StateFlow<AuthUiState> = _state.asStateFlow()

    fun login(login: String, password: String) {
        if (login.isBlank() || password.isBlank()) {
            _state.value = AuthUiState(error = "Заповніть логін і пароль")
            return
        }
        _state.value = AuthUiState(loading = true)
        viewModelScope.launch {
            repo.login(login.trim(), password)
                .onSuccess { _state.value = AuthUiState(success = true) }
                .onFailure { _state.value = AuthUiState(error = it.userMessage()) }
        }
    }

    fun register(firstName: String, lastName: String, email: String, userName: String, password: String) {
        if (listOf(firstName, lastName, email, userName, password).any { it.isBlank() }) {
            _state.value = AuthUiState(error = "Усі поля обов'язкові")
            return
        }
        _state.value = AuthUiState(loading = true)
        viewModelScope.launch {
            repo.register(RegisterRequest(firstName.trim(), lastName.trim(), email.trim(), userName.trim(), password))
                .onSuccess { _state.value = AuthUiState(success = true) }
                .onFailure { _state.value = AuthUiState(error = it.userMessage()) }
        }
    }

    fun reset() {
        _state.value = AuthUiState()
    }
}
