﻿using AuthorizationInterceptor.Extensions.Abstractions.Headers;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthorizationInterceptor.Extensions.Abstractions.Json
{
    internal class AuthorizationHeadersJsonConverter : JsonConverter<AuthorizationHeaders>
    {
        public override AuthorizationHeaders Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var authorizationHeaders = (AuthorizationHeaders)Activator.CreateInstance(typeof(AuthorizationHeaders), true)!;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return authorizationHeaders;

                var propertyName = reader.GetString();
                reader.Read();

                if (reader.TokenType == JsonTokenType.Null)
                    continue;

                switch (propertyName)
                {
                    case "Headers":
                        SetHeaders(ref reader, authorizationHeaders);
                        continue;
                    case "ExpiresIn":
                        var expiresIn = reader.GetString();
                        if (string.IsNullOrEmpty(expiresIn))
                            continue;

                        SetProperty(authorizationHeaders, "ExpiresIn", TimeSpan.Parse(expiresIn));
                        continue;
                    case "AuthenticatedAt":
                        var authenticatedAt = reader.GetString();
                        if (string.IsNullOrEmpty(authenticatedAt))
                            continue;

                        SetProperty(authorizationHeaders, "AuthenticatedAt", DateTimeOffset.Parse(authenticatedAt));
                        continue;
                    case "OAuthHeaders":
                        SetOAuthHeaders(ref reader, options, authorizationHeaders);
                        continue;
                }
            }

            throw new JsonException("Expected EndObject token.");
        }

        public override void Write(Utf8JsonWriter writer, AuthorizationHeaders value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteStartObject("Headers");
            foreach (var item in value)
                writer.WriteString(item.Key, item.Value);

            writer.WriteEndObject();

            writer.WriteString("ExpiresIn", value.ExpiresIn?.ToString());
            writer.WriteString("AuthenticatedAt", value.AuthenticatedAt);

            if (value.OAuthHeaders == null)
            {
                writer.WriteNull("OAuthHeaders");
                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject("OAuthHeaders");
            writer.WriteString("AccessToken", value.OAuthHeaders?.AccessToken);
            writer.WriteString("TokenType", value.OAuthHeaders?.TokenType);
            writer.WriteString("ExpiresIn", value.OAuthHeaders?.ExpiresIn?.ToString());
            writer.WriteString("RefreshToken", value.OAuthHeaders?.RefreshToken);
            writer.WriteString("Scope", value.OAuthHeaders?.Scope);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        private void SetOAuthHeaders(ref Utf8JsonReader reader, JsonSerializerOptions options, AuthorizationHeaders authorizationHeaders)
        {
            var oAuthHeadersJson = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            
            var accessToken = RequireStringProperty(oAuthHeadersJson, "AccessToken");
            if (string.IsNullOrEmpty(accessToken))
                return;

            var tokenType = RequireStringProperty(oAuthHeadersJson, "TokenType");
            if (string.IsNullOrEmpty(tokenType))
                return;

            var expiresInValue = RequireStringProperty(oAuthHeadersJson, "ExpiresIn");
            double? expiresIn = string.IsNullOrEmpty(expiresInValue) ? null : double.Parse(expiresInValue);
            var refreshToken = RequireStringProperty(oAuthHeadersJson, "RefreshToken");
            var scope = RequireStringProperty(oAuthHeadersJson, "Scope");
            SetProperty(authorizationHeaders, "OAuthHeaders", new OAuthHeaders(accessToken, tokenType, expiresIn, refreshToken, scope));
        }

        private void SetHeaders(ref Utf8JsonReader reader, AuthorizationHeaders authorizationHeaders)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var key = reader.GetString();
                reader.Read();
                var value = reader.GetString();
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                authorizationHeaders.Add(key, value);
            }
        }

        private void SetProperty<TValue>(AuthorizationHeaders obj, string propertyName, TValue value)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo?.SetValue(obj, value, null);
        }

        public string? RequireStringProperty(JsonElement oAuthHeadersJson, string propertyName)
        {
            if (!oAuthHeadersJson.TryGetProperty(propertyName, out var property))
                return null;

            return property.GetString();
        }
    }
}
