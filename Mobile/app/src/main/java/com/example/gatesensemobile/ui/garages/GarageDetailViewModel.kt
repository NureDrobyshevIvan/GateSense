package com.example.gatesensemobile.ui.garages

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gatesensemobile.data.api.GarageDto
import com.example.gatesensemobile.data.api.GateStateResponse
import com.example.gatesensemobile.data.api.SensorReadingDto
import com.example.gatesensemobile.data.repository.GarageRepository
import com.example.gatesensemobile.data.repository.GateRepository
import com.example.gatesensemobile.data.repository.SensorRepository
import com.example.gatesensemobile.di.ServiceLocator
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

data class GarageDetailUiState(
    val loading: Boolean = false,
    val garage: GarageDto? = null,
    val gateState: GateStateResponse? = null,
    val latestReadings: List<SensorReadingDto> = emptyList(),
    val alerts: List<SensorReadingDto> = emptyList(),
    val acting: Boolean = false,
    val error: String? = null
)

class GarageDetailViewModel(
    val garageId: Int,
    private val garages: GarageRepository = ServiceLocator.garageRepository,
    private val gates: GateRepository = ServiceLocator.gateRepository,
    private val sensors: SensorRepository = ServiceLocator.sensorRepository
) : ViewModel() {

    private val _state = MutableStateFlow(GarageDetailUiState())
    val state: StateFlow<GarageDetailUiState> = _state.asStateFlow()

    init { refreshAll() }

    fun refreshAll() {
        _state.value = _state.value.copy(loading = true, error = null)
        viewModelScope.launch {
            garages.get(garageId).onSuccess {
                _state.value = _state.value.copy(garage = it)
            }
            gates.state(garageId).onSuccess {
                _state.value = _state.value.copy(gateState = it)
            }
            sensors.latest(garageId).onSuccess {
                _state.value = _state.value.copy(latestReadings = it)
            }
            sensors.alerts(garageId).onSuccess {
                _state.value = _state.value.copy(alerts = it)
            }
            _state.value = _state.value.copy(loading = false)
        }
    }

    fun toggleGate() {
        val current = _state.value.gateState?.state
        viewModelScope.launch {
            _state.value = _state.value.copy(acting = true, error = null)
            val result = if (current.equals("Open", true)) {
                gates.close(garageId)
            } else {
                gates.open(garageId)
            }
            result
                .onSuccess {
                    gates.state(garageId).onSuccess {
                        _state.value = _state.value.copy(gateState = it)
                    }
                }
                .onFailure {
                    _state.value = _state.value.copy(error = it.localizedMessage ?: "Команду не виконано")
                }
            _state.value = _state.value.copy(acting = false)
        }
    }
}
