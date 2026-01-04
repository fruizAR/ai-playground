export interface AskRequest {
  prompt: string;
  temperature?: number;
  maxtokens?: number;
  stream?: boolean;
}

export interface AskResponse {
  response: string;
  tokensused?: number;
  finishReason?: string;
}

export interface StatusResponse {
  status: string;
  version: string;
  openaiconnected: boolean;
}

export interface StreamChunk {
  content?: string;
  error?: string;
  finishReason?: string;
}

export interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: Date;
  isStreaming?: boolean;
  finishReason?: string;
  isTruncated?: boolean;
}
