package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.GateCommandRequest
import com.example.gatesensemobile.data.api.GateStateResponse

class GateRepository(private val api: ApiService) {
    suspend fun open(garageId: Int, accessKeyToken: String? = null): Result<Unit> = runCatching {
        api.openGate(garageId, GateCommandRequest(accessKeyToken))
    }

    suspend fun close(garageId: Int, accessKeyToken: String? = null): Result<Unit> = runCatching {
        api.closeGate(garageId, GateCommandRequest(accessKeyToken))
    }

    suspend fun state(garageId: Int): Result<GateStateResponse> = runCatching {
        api.getGateState(garageId).data
    }
}
