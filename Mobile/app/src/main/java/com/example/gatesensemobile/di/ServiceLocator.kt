package com.example.gatesensemobile.di

import android.content.Context
import com.example.gatesensemobile.data.api.ApiClient
import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.auth.TokenStore
import com.example.gatesensemobile.data.repository.AccessRepository
import com.example.gatesensemobile.data.repository.AuthRepository
import com.example.gatesensemobile.data.repository.GarageRepository
import com.example.gatesensemobile.data.repository.GateRepository
import com.example.gatesensemobile.data.repository.LogRepository
import com.example.gatesensemobile.data.repository.SensorRepository

object ServiceLocator {

    private lateinit var appContext: Context

    val tokenStore: TokenStore by lazy { TokenStore(appContext) }
    val api: ApiService by lazy { ApiClient.create(tokenStore) }

    val authRepository: AuthRepository by lazy { AuthRepository(api, tokenStore) }
    val garageRepository: GarageRepository by lazy { GarageRepository(api) }
    val gateRepository: GateRepository by lazy { GateRepository(api) }
    val sensorRepository: SensorRepository by lazy { SensorRepository(api) }
    val accessRepository: AccessRepository by lazy { AccessRepository(api) }
    val logRepository: LogRepository by lazy { LogRepository(api) }

    fun init(context: Context) {
        appContext = context.applicationContext
    }
}
