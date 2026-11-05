import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import Icon, { IoniconsIconName } from '@react-native-vector-icons/ionicons';
import DashboardScreen from '../screens/DashboardScreen';
import RoutesScreen from '../screens/RoutesScreen';
import WalletTabNavigator from '../navigation/WalletTabNavigator'; 
import ProfileScreen from '../screens/ProfileScreen';
import MapScreen from '../screens/MapScreen';

const Tab = createBottomTabNavigator();

const MainTabNavigator = () => {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarActiveTintColor: "#346699",
        tabBarInactiveTintColor: 'gray',
        tabBarIcon: ({ color, size }) => {
          var iconName: IoniconsIconName = 'home-outline';

          if (route.name === 'Dashboard') iconName = 'home-outline';
          else if (route.name === 'Routes') iconName = 'git-branch-outline';
          else if (route.name === 'Map') iconName = 'map-outline';
          else if (route.name === 'PaymentHistory') iconName = 'wallet-outline';
          else if (route.name === 'Profile') iconName = 'person-outline';

          return <Icon name={iconName} size={size} color={color} />;
        },
      })}
    >
      <Tab.Screen
        name="Dashboard"
        component={DashboardScreen}
        options={{ title: 'Pulpit' }}
      />
      <Tab.Screen
        name="Routes"
        component={RoutesScreen}
        options={{ title: 'Trasy' }}
      />
          
      <Tab.Screen
        name="Map"
        component={MapScreen}
        options={{ title: 'Mapa' }}
      />

      <Tab.Screen
        name="PaymentHistory"
        component={WalletTabNavigator}
        options={{ title: 'Portfel' }} 
      />
      
      <Tab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{ title: 'Profil' }}
      />
    </Tab.Navigator>
  );
};

export default MainTabNavigator;
