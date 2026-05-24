package com.example.gatesensemobile.ui.garages

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gatesensemobile.data.api.GarageDto
import com.example.gatesensemobile.data.repository.AuthRepository
import com.example.gatesensemobile.data.repository.GarageRepository
import com.example.gatesensemobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class GaragesUiState(
    val loading: Boolean = false,
    val items: List<GarageDto> = emptyList(),
    val error: String? = null,
    val userName: String? = null,
    val createBusy: Boolean = false,
    val justLoggedOut: Boolean = false
)

class GaragesViewModel(
    private val garages: GarageRepository = ServiceLocator.garageRepository,
    private val auth: AuthRepository = ServiceLocator.authRepository
) : ViewModel() {

    private val _state = MutableStateFlow(GaragesUiState())
    val state: StateFlow<GaragesUiState> = _state.asStateFlow()

    init {
        viewModelScope.launch {
            auth.userNameFlow().collect { name ->
                _state.value = _state.value.copy(userName = name)
            }
        }
        refresh()
    }

    fun refresh() {
        _state.value = _state.value.copy(loading = true, error = null)
        viewModelScope.launch {
            garages.list()
                .onSuccess { _state.value = _state.value.copy(loading = false, items = it) }
                .onFailure {
                    _state.value = _state.value.copy(loading = false, error = it.localizedMessage ?: "Не вдалося завантажити")
                }
        }
    }

    fun createGarage(name: String, address: String?, onCreated: () -> Unit) {
        if (name.isBlank()) {
            _state.value = _state.value.copy(error = "Назва обов'язкова")
            return
        }
        _state.value = _state.value.copy(createBusy = true, error = null)
        viewModelScope.launch {
            garages.create(name.trim(), address?.takeIf { it.isNotBlank() }, null)
                .onSuccess {
                    _state.value = _state.value.copy(createBusy = false)
                    onCreated()
                    refresh()
                }
                .onFailure {
                    _state.value = _state.value.copy(createBusy = false, error = it.localizedMessage ?: "Не вдалося створити")
                }
        }
    }

    fun delete(id: Int) {
        viewModelScope.launch {
            garages.delete(id).onSuccess { refresh() }
        }
    }

    fun logout() {
        viewModelScope.launch {
            auth.logout()
            _state.value = _state.value.copy(justLoggedOut = true)
        }
    }
}
