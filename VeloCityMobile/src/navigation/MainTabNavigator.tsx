import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import Icon, { IoniconsIconName } from '@react-native-vector-icons/ionicons';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { RootStackParamList } from './types';
import { getBaseUrl } from '../api/config';
import DashboardScreen from '../screens/DashboardScreen';
import RoutesScreen from '../screens/RoutesScreen';
import WalletTabNavigator from '../navigation/WalletTabNavigator'; 
import ProfileScreen from '../screens/ProfileScreen';
import MapScreen from '../screens/MapScreen';
import ConnectionSearchScreen from '../screens/ConnectionSearchScreen';

const Tab = createBottomTabNavigator();

type RootNavigation = NativeStackNavigationProp<RootStackParamList>;

function ConnectionSearchTab(): React.JSX.Element {
  const navigation = useNavigation<RootNavigation>();

  return (
    <ConnectionSearchScreen
      apiBaseUrl={getBaseUrl()}
      onSelectConnection={(connection) => {
        navigation.navigate('PurchaseTicket', {
          connection,
        });
      }}
    />
  );
}

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
        component={ConnectionSearchTab}
        options={{ title: 'Połączenia' }}
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
