package com.example.gatesensemobile.data.api

import kotlinx.serialization.Serializable

@Serializable
data class ApiResponse<T>(val data: T)

// ---------- Auth ----------

@Serializable
data class LoginRequest(val login: String, val password: String)

@Serializable
data class RegisterRequest(
    val firstName: String,
    val lastName: String,
    val email: String,
    val userName: String,
    val password: String
)

@Serializable
data class LoginResponse(
    val userName: String? = null,
    val email: String? = null,
    val role: String,
    val accessToken: String? = null,
    val refreshToken: String? = null
)

@Serializable
data class UserProfileDto(
    val id: Int? = null,
    val userName: String? = null,
    val email: String? = null,
    val firstName: String? = null,
    val lastName: String? = null
)

// ---------- Garages ----------

@Serializable
data class GarageDto(
    val id: Int,
    val name: String,
    val address: String? = null,
    val timeZone: String? = null,
    val ownerId: Int? = null
)

@Serializable
data class CreateGarageRequest(
    val name: String,
    val address: String? = null,
    val timeZone: String? = null
)

@Serializable
data class UpdateGarageRequest(
    val name: String,
    val address: String? = null,
    val timeZone: String? = null
)

// ---------- Gate ----------

@Serializable
data class GateCommandRequest(val accessKeyToken: String? = null)

@Serializable
data class GateStateResponse(
    val garageId: Int,
    val state: String,
    val lastAction: String? = null,
    val lastActionTime: String? = null
)

// ---------- Sensors ----------

@Serializable
data class SensorReadingDto(
    val id: Int? = null,
    val deviceId: Int? = null,
    val sensorType: Int? = null,
    val value: Double,
    val unit: String? = null,
    val recordedOn: String? = null
) {
    val sensorTypeLabel: String get() = when (sensorType) {
        0 -> "CO"
        1 -> "Smoke"
        2 -> "Temperature"
        3 -> "Humidity"
        else -> "?"
    }
}

@Serializable
data class PaginatedDto<T>(
    val currentPage: Int = 1,
    val totalPages: Int = 1,
    val pageSize: Int = 0,
    val totalCount: Int = 0,
    val items: List<T> = emptyList()
)

// ---------- Access ----------

@Serializable
data class GarageMemberDto(
    val id: Int,
    val garageId: Int,
    val userId: Int? = null,
    val accessLevel: Int? = null,
    val expiresOn: String? = null,
    val user: UserShortDto? = null
) {
    val accessLevelLabel: String get() = when (accessLevel) {
        0 -> "Owner"
        1 -> "Family"
        2 -> "Guest"
        else -> "?"
    }
}

@Serializable
data class UserShortDto(
    val id: Int? = null,
    val userName: String? = null,
    val email: String? = null
)

@Serializable
data class AssignFamilyAccessRequest(
    val garageId: Int,
    val email: String
)

@Serializable
data class CreateGuestAccessRequest(
    val garageId: Int,
    val recipientName: String,
    val recipientEmail: String? = null,
    val expiresOn: String? = null
)

@Serializable
data class CreateGuestAccessResponse(
    val id: Int,
    val token: String,
    val expiresOn: String? = null
)

// ---------- Logs ----------

@Serializable
data class GateEventDto(
    val id: Int,
    val garageId: Int,
    val initiatorUserId: Int? = null,
    val accessKeyId: Int? = null,
    val triggerSource: Int? = null,
    val action: Int? = null,
    val result: Int? = null,
    val failureReason: String? = null,
    val createdOn: String? = null
) {
    val triggerSourceLabel: String get() = when (triggerSource) {
        0 -> "Owner"
        1 -> "Family"
        2 -> "Guest"
        3 -> "System"
        else -> "?"
    }
    val actionLabel: String get() = when (action) {
        0 -> "Open"
        1 -> "Close"
        else -> "?"
    }
    val resultLabel: String get() = when (result) {
        0 -> "Success"
        1 -> "Failure"
        else -> "?"
    }
}
