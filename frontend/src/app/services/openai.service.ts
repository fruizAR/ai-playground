import {
  AskRequest,
  AskResponse,
  StatusResponse,
  StreamChunk,
} from "../models/chat.models";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable, Subject } from "rxjs";

import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { map } from "rxjs/operators";

@Injectable({
  providedIn: "root",
})
export class OpenAIService {
  private apiUrl = environment.apiUrl + "/api/chat";

  constructor(private http: HttpClient) {}

  /**
   * Envía una solicitud a OpenAI sin streaming
   */
  askWithoutStreaming(request: AskRequest): Observable<AskResponse> {
    const payload = {
      ...request,
      stream: false,
    };

    return this.http.post<AskResponse>(`${this.apiUrl}/ask`, payload);
  }

  /**
   * Envía una solicitud a OpenAI con streaming usando Server-Sent Events
   */
  askWithStreaming(request: AskRequest): Observable<StreamChunk> {
    const payload = {
      ...request,
      stream: true,
    };

    return new Observable<StreamChunk>((observer) => {
      const eventSource = this.createEventSource(`${this.apiUrl}/ask`, payload);

      eventSource.onmessage = (event) => {
        if (event.data === "[DONE]") {
          observer.complete();
          eventSource.close();
          return;
        }

        try {
          const chunk: StreamChunk = JSON.parse(event.data);

          if (chunk.error) {
            observer.error(new Error(chunk.error));
            eventSource.close();
            return;
          }

          observer.next(chunk);
        } catch (error) {
          console.error("Error parsing SSE data:", error);
        }
      };

      eventSource.onerror = (error) => {
        observer.error(error);
        eventSource.close();
      };

      // Cleanup al desuscribirse
      return () => {
        eventSource.close();
      };
    });
  }

  /**
   * Crea una conexión EventSource para Server-Sent Events
   * Workaround para poder enviar POST con body
   */
  private createEventSource(url: string, body: any): EventSource {
    // Para SSE con POST, necesitamos hacer una petición especial
    // Angular no soporta SSE con POST nativamente, así que usamos fetch
    const fetchSSE = async () => {
      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
      });

      return response;
    };

    // Creamos un EventSource customizado
    // Nota: Esta es una implementación simplificada
    // En producción, considera usar una librería como eventsource-parser
    const reader = fetchSSE().then((response) => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.body?.getReader();
    });

    // Retornamos un objeto compatible con EventSource
    const eventSource = new EventTarget() as any;
    eventSource.close = () => {};

    reader
      .then((reader) => {
        if (!reader) return;

        const decoder = new TextDecoder();
        const processStream = async () => {
          try {
            while (true) {
              const { done, value } = await reader.read();
              if (done) break;

              const text = decoder.decode(value, { stream: true });
              const lines = text.split("\n");

              for (const line of lines) {
                if (line.startsWith("data: ")) {
                  const data = line.slice(6);
                  const event = new MessageEvent("message", { data });
                  if (eventSource.onmessage) {
                    eventSource.onmessage(event);
                  }
                }
              }
            }
          } catch (error) {
            if (eventSource.onerror) {
              eventSource.onerror(error);
            }
          }
        };

        processStream();
        eventSource.close = () => reader.cancel();
      })
      .catch((error) => {
        if (eventSource.onerror) {
          eventSource.onerror(error);
        }
      });

    return eventSource;
  }

  /**
   * Obtiene el estado del servicio backend
   */
  getStatus(): Observable<StatusResponse> {
    return this.http.get<StatusResponse>(`${this.apiUrl}/status`);
  }

  /**
   * Obtiene los logs del servicio
   */
  getLogs(limit: number = 100, level?: string): Observable<any[]> {
    let url = `${this.apiUrl}/logs?limit=${limit}`;
    if (level) {
      url += `&level=${level}`;
    }
    return this.http.get<any[]>(url);
  }
}
