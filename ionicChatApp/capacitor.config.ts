import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.chatapp.app',
  appName: 'ionicChatApp',
  webDir: 'www',
  bundledWebRuntime: false,
  server: {
    hostname: 'chatapp.com',
    androidScheme: 'http',
    iosScheme: 'httpsionic',
  },
};

export default config;
