package com.example.gatesensemobile.ui.events

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gatesensemobile.data.api.GateEventDto
import com.example.gatesensemobile.data.repository.LogRepository
import com.example.gatesensemobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class EventLogUiState(
    val loading: Boolean = false,
    val events: List<GateEventDto> = emptyList(),
    val error: String? = null
)

class EventLogViewModel(
    val garageId: Int,
    private val repo: LogRepository = ServiceLocator.logRepository
) : ViewModel() {

    private val _state = MutableStateFlow(EventLogUiState())
    val state: StateFlow<EventLogUiState> = _state.asStateFlow()

    init { refresh() }

    fun refresh() {
        _state.value = _state.value.copy(loading = true, error = null)
        viewModelScope.launch {
            repo.gate(garageId)
                .onSuccess { _state.value = EventLogUiState(events = it) }
                .onFailure { _state.value = EventLogUiState(error = it.localizedMessage) }
        }
    }
}
