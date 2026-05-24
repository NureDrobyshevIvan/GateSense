package com.example.gatesensemobile.ui.access

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gatesensemobile.data.api.GarageMemberDto
import com.example.gatesensemobile.data.repository.AccessRepository
import com.example.gatesensemobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class AccessUiState(
    val loading: Boolean = false,
    val members: List<GarageMemberDto> = emptyList(),
    val error: String? = null,
    val busy: Boolean = false,
    val lastGuestToken: String? = null
)

class AccessViewModel(
    val garageId: Int,
    private val repo: AccessRepository = ServiceLocator.accessRepository
) : ViewModel() {

    private val _state = MutableStateFlow(AccessUiState())
    val state: StateFlow<AccessUiState> = _state.asStateFlow()

    init { refresh() }

    fun refresh() {
        _state.value = _state.value.copy(loading = true, error = null)
        viewModelScope.launch {
            repo.members(garageId)
                .onSuccess { _state.value = _state.value.copy(loading = false, members = it) }
                .onFailure { _state.value = _state.value.copy(loading = false, error = it.localizedMessage) }
        }
    }

    fun assignFamily(email: String) {
        if (email.isBlank()) return
        _state.value = _state.value.copy(busy = true, error = null)
        viewModelScope.launch {
            repo.assignFamily(garageId, email.trim())
                .onSuccess { refresh() }
                .onFailure { _state.value = _state.value.copy(busy = false, error = it.localizedMessage) }
            _state.value = _state.value.copy(busy = false)
        }
    }

    fun createGuest(name: String) {
        if (name.isBlank()) return
        _state.value = _state.value.copy(busy = true, error = null, lastGuestToken = null)
        viewModelScope.launch {
            repo.createGuest(garageId, name.trim())
                .onSuccess { _state.value = _state.value.copy(busy = false, lastGuestToken = it.token) }
                .onFailure { _state.value = _state.value.copy(busy = false, error = it.localizedMessage) }
        }
    }

    fun revoke(accessId: Int) {
        viewModelScope.launch {
            repo.revoke(accessId).onSuccess { refresh() }
        }
    }

    fun clearGuestToken() {
        _state.value = _state.value.copy(lastGuestToken = null)
    }
}
