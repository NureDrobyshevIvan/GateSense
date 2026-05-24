package com.example.gatesensemobile.data.auth

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

private val Context.dataStore by preferencesDataStore(name = "auth_prefs")

class TokenStore(private val appContext: Context) {

    private val accessKey = stringPreferencesKey("access_token")
    private val refreshKey = stringPreferencesKey("refresh_token")
    private val userNameKey = stringPreferencesKey("user_name")

    val accessTokenFlow: Flow<String?> = appContext.dataStore.data.map { it[accessKey] }
    val userNameFlow: Flow<String?> = appContext.dataStore.data.map { it[userNameKey] }

    suspend fun accessToken(): String? = appContext.dataStore.data.first()[accessKey]
    suspend fun refreshToken(): String? = appContext.dataStore.data.first()[refreshKey]

    suspend fun save(access: String?, refresh: String?, userName: String?) {
        appContext.dataStore.edit { prefs ->
            access?.let { prefs[accessKey] = it } ?: prefs.remove(accessKey)
            refresh?.let { prefs[refreshKey] = it } ?: prefs.remove(refreshKey)
            userName?.let { prefs[userNameKey] = it } ?: prefs.remove(userNameKey)
        }
    }

    suspend fun clear() {
        appContext.dataStore.edit { it.clear() }
    }
}
