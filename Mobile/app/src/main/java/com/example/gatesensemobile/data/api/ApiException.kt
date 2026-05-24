package com.example.gatesensemobile.data.api

import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import retrofit2.HttpException

@Serializable
data class ProblemDetails(
    val type: String? = null,
    val title: String? = null,
    val status: Int? = null,
    val detail: String? = null,
    val errors: Map<String, List<String>>? = null
)

private val json = Json { ignoreUnknownKeys = true; coerceInputValues = true; explicitNulls = false }

fun Throwable.userMessage(): String {
    if (this is HttpException) {
        val body = response()?.errorBody()?.string().orEmpty()
        if (body.isNotBlank()) {
            runCatching {
                val pd = json.decodeFromString(ProblemDetails.serializer(), body)
                val parts = buildList {
                    pd.title?.let { add(it) }
                    pd.detail?.let { add(it) }
                    pd.errors?.forEach { (k, v) -> add("$k: ${v.joinToString()}") }
                }
                if (parts.isNotEmpty()) return parts.joinToString("\n")
            }
            return "HTTP ${code()}: $body"
        }
        return "HTTP ${code()} ${message()}"
    }
    return localizedMessage ?: toString()
}
