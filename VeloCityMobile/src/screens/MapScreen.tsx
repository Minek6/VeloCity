
import React, {useEffect, useMemo, useState} from 'react';
import {
  ActivityIndicator,
  StyleSheet,
  Text,
  View,
  Pressable,
} from 'react-native';

import {
  Map,
  Camera,
  GeoJSONSource,
  Layer,
} from '@maplibre/maplibre-react-native';

import apiService from '../api/apiService';
import type {MapStopDto} from '../types/map';

const NOWY_SACZ = {
  longitude: 20.697,
  latitude: 49.621,
};

export default function MapScreen() {
  const [stops, setStops] = useState<MapStopDto[]>([]);
  const [selectedStop, setSelectedStop] =
    useState<MapStopDto | null>(null);

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

  // Zamieniamy dane z API na GeoJSON.
  const stopsGeoJSON = useMemo(() => ({
    type: 'FeatureCollection' as const,
    features: stops.map(stop => ({
      type: 'Feature' as const,
      geometry: {
        type: 'Point' as const,
        coordinates: [stop.longitude, stop.latitude],
      },
      properties: {
        stopId: stop.id,
      },
    })),
  }), [stops]);

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

        <GeoJSONSource
          key="velocity-stops"
          id="velocity-stops"
          data={stopsGeoJSON}
          cluster={true}
          clusterRadius={55}
          clusterMaxZoom={15}
          onPress={event => {
            const feature = event.nativeEvent.features?.[0];

            if (!feature) {
              return;
            }

            // Dotknięcie grupy nie wybiera przystanku.
            if (feature.properties?.cluster) {
              return;
            }

            const stopId = Number(
              feature.properties?.stopId,
            );

            const stop = stops.find(s => s.id === stopId);

            if (stop) {
              setSelectedStop(stop);
            }
          }}
        >
          {/* Okręgi reprezentujące grupy przystanków */}
          <Layer
            key="stop-clusters"
            id="stop-clusters"
            type="circle"
            filter={['has', 'point_count']}
            paint={{
              'circle-color': '#346699',
              'circle-radius': [
                'step',
                ['get', 'point_count'],
                20,
                20, 25,
                50, 32,
              ],
              'circle-stroke-width': 2,
              'circle-stroke-color': '#FFFFFF',
            }}
          />
          
          <Layer
            key="stop-cluster-count"
            id="stop-cluster-count"
            type="symbol"
            filter={['has', 'point_count']}
            layout={{
              'text-field': ['to-string', ['get', 'point_count']],
              'text-font': ['Noto Sans Regular'],
              'text-size': 14,
              'text-allow-overlap': true,
              'text-ignore-placement': true,
            }}
            paint={{
              'text-color': '#FFFFFF',
            }}
          />

          {/* Pojedyncze przystanki */}
          <Layer
            key="single-stops"
            id="single-stops"
            type="circle"
            filter={['!', ['has', 'point_count']]}
            paint={{
              'circle-color': '#FFFFFF',
              'circle-radius': 9,
              'circle-stroke-width': 3,
              'circle-stroke-color': '#346699',
            }}
          />
        </GeoJSONSource>
      </Map>

      {selectedStop && (
        <View style={styles.stopInfo}>
          <Text style={styles.stopName}>
            🚏 {selectedStop.name}
          </Text>

          <Text>
            ID przystanku: {selectedStop.id}
          </Text>

          <Pressable onPress={() => setSelectedStop(null)}>
            <Text style={styles.closeButton}>
              Zamknij
            </Text>
          </Pressable>
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
