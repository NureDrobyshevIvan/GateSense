package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.AssignFamilyAccessRequest
import com.example.gatesensemobile.data.api.CreateGuestAccessRequest
import com.example.gatesensemobile.data.api.CreateGuestAccessResponse
import com.example.gatesensemobile.data.api.GarageMemberDto

class AccessRepository(private val api: ApiService) {

    suspend fun members(garageId: Int): Result<List<GarageMemberDto>> = runCatching {
        api.getMembers(garageId).data
    }

    suspend fun assignFamily(garageId: Int, email: String): Result<Unit> = runCatching {
        api.assignFamily(AssignFamilyAccessRequest(garageId, email))
    }

    suspend fun createGuest(
        garageId: Int,
        recipientName: String,
        email: String? = null,
        expiresOn: String? = null
    ): Result<CreateGuestAccessResponse> = runCatching {
        api.createGuest(CreateGuestAccessRequest(garageId, recipientName, email, expiresOn)).data
    }

    suspend fun revoke(accessId: Int): Result<Unit> = runCatching {
        api.revokeAccess(accessId)
    }
}
