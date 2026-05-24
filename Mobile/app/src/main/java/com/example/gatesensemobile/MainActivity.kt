package com.example.gatesensemobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import com.example.gatesensemobile.ui.nav.GateSenseNavGraph
import com.example.gatesensemobile.ui.theme.GateSenseMobileTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            GateSenseMobileTheme {
                GateSenseNavGraph()
            }
        }
    }
}
