package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.LoginRequest
import com.example.gatesensemobile.data.api.RegisterRequest
import com.example.gatesensemobile.data.auth.TokenStore

class AuthRepository(
    private val api: ApiService,
    private val tokenStore: TokenStore
) {
    suspend fun login(login: String, password: String): Result<String> = runCatching {
        val res = api.login(LoginRequest(login, password)).data
        tokenStore.save(res.accessToken, res.refreshToken, res.userName)
        res.userName ?: login
    }

    suspend fun register(req: RegisterRequest): Result<Unit> = runCatching {
        api.register(req)
    }

    suspend fun logout(): Result<Unit> = runCatching {
        runCatching { api.logout() }
        tokenStore.clear()
    }

    fun userNameFlow() = tokenStore.userNameFlow
    fun accessTokenFlow() = tokenStore.accessTokenFlow
}
