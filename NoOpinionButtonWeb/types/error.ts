export enum ApiErrorType {
  BadRequest = 'BadRequest',
  Unauthorized = 'Unauthorized',
  NotFound = 'NotFound',
  Server = 'Server',
  WebSocketConnection = 'WebSocketConnection',
  WebSocketMessage = 'WebSocketMessage'
}

export interface ApiError {
  type: ApiErrorType
  message: string
  statusCode: number
}