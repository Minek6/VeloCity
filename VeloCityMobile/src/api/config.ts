import { Platform } from 'react-native';

const getBaseUrl = (): string => {
  if (__DEV__) {
    // Development
    if (Platform.OS === 'android') {
      return 'http://localhost:8080';
    } else if (Platform.OS === 'ios') {
      return 'https://velocity.bieda.it';
    }
  }

  return 'https://velocity.bieda.it';
};

export const API_BASE_URL = getBaseUrl();
