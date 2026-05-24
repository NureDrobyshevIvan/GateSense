package com.example.gatesensemobile.ui.nav

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.gatesensemobile.di.ServiceLocator
import com.example.gatesensemobile.ui.access.AccessScreen
import com.example.gatesensemobile.ui.auth.LoginScreen
import com.example.gatesensemobile.ui.auth.RegisterScreen
import com.example.gatesensemobile.ui.events.EventLogScreen
import com.example.gatesensemobile.ui.garages.GarageDetailScreen
import com.example.gatesensemobile.ui.garages.GaragesScreen

object Routes {
    const val LOGIN = "login"
    const val REGISTER = "register"
    const val GARAGES = "garages"
    const val GARAGE_DETAIL = "garage/{id}"
    const val ACCESS = "access/{id}"
    const val EVENTS = "events/{id}"
    fun garageDetail(id: Int) = "garage/$id"
    fun access(id: Int) = "access/$id"
    fun events(id: Int) = "events/$id"
}

@Composable
fun GateSenseNavGraph() {
    val nav = rememberNavController()
    val tokenStore = remember { ServiceLocator.tokenStore }
    val token by tokenStore.accessTokenFlow.collectAsState(initial = null)

    LaunchedEffect(token) {
        // If token cleared (logout) and we're not already on login — go to login.
        val current = nav.currentBackStackEntry?.destination?.route
        if (token.isNullOrBlank() && current != Routes.LOGIN && current != Routes.REGISTER) {
            nav.navigate(Routes.LOGIN) { popUpTo(0) }
        }
    }

    val startDestination = if (!token.isNullOrBlank()) Routes.GARAGES else Routes.LOGIN

    NavHost(navController = nav, startDestination = startDestination) {

        composable(Routes.LOGIN) {
            LoginScreen(
                onLoggedIn = { nav.navigate(Routes.GARAGES) { popUpTo(Routes.LOGIN) { inclusive = true } } },
                onGoToRegister = { nav.navigate(Routes.REGISTER) }
            )
        }

        composable(Routes.REGISTER) {
            RegisterScreen(
                onRegistered = { nav.popBackStack() },
                onBack = { nav.popBackStack() }
            )
        }

        composable(Routes.GARAGES) {
            GaragesScreen(
                onOpen = { id -> nav.navigate(Routes.garageDetail(id)) },
                onLoggedOut = { nav.navigate(Routes.LOGIN) { popUpTo(0) } }
            )
        }

        composable(
            Routes.GARAGE_DETAIL,
            arguments = listOf(navArgument("id") { type = NavType.IntType })
        ) { entry ->
            val id = entry.arguments?.getInt("id") ?: return@composable
            GarageDetailScreen(
                garageId = id,
                onBack = { nav.popBackStack() },
                onOpenAccess = { nav.navigate(Routes.access(it)) },
                onOpenEvents = { nav.navigate(Routes.events(it)) }
            )
        }

        composable(
            Routes.ACCESS,
            arguments = listOf(navArgument("id") { type = NavType.IntType })
        ) { entry ->
            val id = entry.arguments?.getInt("id") ?: return@composable
            AccessScreen(garageId = id, onBack = { nav.popBackStack() })
        }

        composable(
            Routes.EVENTS,
            arguments = listOf(navArgument("id") { type = NavType.IntType })
        ) { entry ->
            val id = entry.arguments?.getInt("id") ?: return@composable
            EventLogScreen(garageId = id, onBack = { nav.popBackStack() })
        }
    }
}
