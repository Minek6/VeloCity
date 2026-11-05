
import React, {useEffect, useState} from 'react';
import {ActivityIndicator, StyleSheet, Text, View} from 'react-native';
import {Map, Camera, Marker} from '@maplibre/maplibre-react-native';

import apiService from '../api/apiService';
import type {MapStopDto} from '../types/map';

const NOWY_SACZ = {
  longitude: 20.697,
  latitude: 49.621,
};

export default function MapScreen() {
  const [stops, setStops] = useState<MapStopDto[]>([]);
  const [selectedStop, setSelectedStop] = useState<MapStopDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;

    const loadStops = async () => {
      try {
        const data = await apiService.getMapStops();

        if (mounted) {
          setStops(data);
          setError(null);
          console.log('Map stops loaded:', data.length);
        }
      } catch (err) {
        if (mounted) {
          setError('Nie udało się pobrać przystanków.');
          console.error('Map stops error:', err);
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    loadStops();

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <View style={styles.container}>
      <Map
        style={styles.map}
        mapStyle="https://tiles.openfreemap.org/styles/liberty"
      >
        <Camera
          initialViewState={{
            center: [
              NOWY_SACZ.longitude,
              NOWY_SACZ.latitude,
            ],
            zoom: 13,
          }}
        />
        {stops.map(stop => (
        <Marker
          key={stop.id}
          id={`stop-${stop.id}`}
          lngLat={[stop.longitude, stop.latitude]}
          onPress={() => setSelectedStop(stop)}
        >
          <View style={styles.marker}>
            <Text style={styles.markerText}>🚏</Text>
          </View>
        </Marker>
        ))}
      </Map>
      
      {selectedStop && (
        <View style={styles.stopInfo}>
          <Text style={styles.stopName}>
            🚏 {selectedStop.name}
          </Text>

          <Text>
            ID przystanku: {selectedStop.id}
          </Text>

          <Text
            style={styles.closeButton}
            onPress={() => setSelectedStop(null)}
          >
            Zamknij
          </Text>
        </View>
      )}

      {loading && (
        <View style={styles.message}>
          <ActivityIndicator size="small" />
          <Text>Ładowanie przystanków...</Text>
        </View>
      )}

      {error && (
        <View style={styles.message}>
          <Text>{error}</Text>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  map: {
    flex: 1,
  },
  message: {
    position: 'absolute',
    top: 20,
    alignSelf: 'center',
    backgroundColor: 'white',
    padding: 12,
    borderRadius: 8,
    elevation: 3,
  },
  marker: {
    backgroundColor: 'white',
    borderRadius: 20,
    padding: 6,
    borderWidth: 2,
    borderColor: '#346699',
    alignItems: 'center',
    justifyContent: 'center',
  },
  markerText: {
    fontSize: 18,
  },
  stopInfo: {
    position: 'absolute',
    bottom: 20,
    left: 15,
    right: 15,
    backgroundColor: 'white',
    padding: 16,
    borderRadius: 12,
    elevation: 5,
  },
  stopName: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 8,
    color: '#346699',
  },
  closeButton: {
    marginTop: 12,
    color: '#346699',
    fontWeight: 'bold',
  },

});
