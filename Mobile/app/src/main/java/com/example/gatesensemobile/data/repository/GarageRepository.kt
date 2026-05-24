package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.CreateGarageRequest
import com.example.gatesensemobile.data.api.GarageDto
import com.example.gatesensemobile.data.api.UpdateGarageRequest

class GarageRepository(private val api: ApiService) {

    suspend fun list(): Result<List<GarageDto>> = runCatching {
        api.getGarages().data
    }

    suspend fun get(id: Int): Result<GarageDto> = runCatching {
        api.getGarage(id).data
    }

    suspend fun create(name: String, address: String?, timeZone: String?): Result<Int> = runCatching {
        api.createGarage(CreateGarageRequest(name, address, timeZone)).data
    }

    suspend fun update(id: Int, name: String, address: String?, timeZone: String?): Result<Unit> = runCatching {
        api.updateGarage(id, UpdateGarageRequest(name, address, timeZone))
    }

    suspend fun delete(id: Int): Result<Unit> = runCatching { api.deleteGarage(id) }
}
