package com.example.gatesensemobile

import android.app.Application
import com.example.gatesensemobile.di.ServiceLocator

class GateSenseApp : Application() {
    override fun onCreate() {
        super.onCreate()
        ServiceLocator.init(this)
    }
}
