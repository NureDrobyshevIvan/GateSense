package com.example.gatesensemobile.data.api

import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

interface ApiService {

    // ---------- Auth ----------

    @POST("Auth/register")
    suspend fun register(@Body body: RegisterRequest)

    @POST("Auth/login")
    suspend fun login(@Body body: LoginRequest): ApiResponse<LoginResponse>

    @GET("Auth/refresh")
    suspend fun refresh(): ApiResponse<LoginResponse>

    @GET("Auth/logout")
    suspend fun logout()

    @GET("Auth/profile")
    suspend fun profile(): ApiResponse<UserProfileDto>

    // ---------- Garages ----------

    @GET("garages")
    suspend fun getGarages(): ApiResponse<List<GarageDto>>

    @GET("garages/{id}")
    suspend fun getGarage(@Path("id") id: Int): ApiResponse<GarageDto>

    @POST("garages")
    suspend fun createGarage(@Body body: CreateGarageRequest): ApiResponse<Int>

    @PUT("garages/{id}")
    suspend fun updateGarage(@Path("id") id: Int, @Body body: UpdateGarageRequest)

    @DELETE("garages/{id}")
    suspend fun deleteGarage(@Path("id") id: Int)

    // ---------- Gate ----------

    @POST("garages/{id}/gate/open")
    suspend fun openGate(@Path("id") id: Int, @Body body: GateCommandRequest)

    @POST("garages/{id}/gate/close")
    suspend fun closeGate(@Path("id") id: Int, @Body body: GateCommandRequest)

    @GET("garages/{id}/gate/state")
    suspend fun getGateState(@Path("id") id: Int): ApiResponse<GateStateResponse>

    // ---------- Sensors ----------

    @GET("garages/{id}/sensors/latest")
    suspend fun getLatestReadings(@Path("id") id: Int): ApiResponse<List<SensorReadingDto>>

    @GET("garages/{id}/sensors/alerts")
    suspend fun getActiveAlerts(@Path("id") id: Int): ApiResponse<List<SensorReadingDto>>

    @GET("garages/{id}/sensors/history")
    suspend fun getSensorHistory(
        @Path("id") id: Int,
        @Query("PageNumber") page: Int = 1,
        @Query("PageSize") pageSize: Int = 50,
        @Query("SensorType") sensorType: String? = null
    ): ApiResponse<PaginatedDto<SensorReadingDto>>

    // ---------- Access ----------

    @GET("access/garages/{id}/members")
    suspend fun getMembers(@Path("id") id: Int): ApiResponse<List<GarageMemberDto>>

    @POST("access/family")
    suspend fun assignFamily(@Body body: AssignFamilyAccessRequest)

    @POST("access/guest")
    suspend fun createGuest(@Body body: CreateGuestAccessRequest): ApiResponse<CreateGuestAccessResponse>

    @DELETE("access/{id}")
    suspend fun revokeAccess(@Path("id") id: Int)

    // ---------- Logs ----------

    @GET("garages/{id}/logs/gate")
    suspend fun getGateLogs(
        @Path("id") id: Int,
        @Query("PageNumber") page: Int = 1,
        @Query("PageSize") pageSize: Int = 50
    ): ApiResponse<PaginatedDto<GateEventDto>>
}
