:: List Ads
C:\Users\nholder\Downloads\curl\curl -G ^
-d "fields=name,id,creative" ^
-d "access_token=CAAGjcNa36u0BAAyZCkAAIrEuaUZAsYio9xki94nsFAka71ySOE9CJR1fOMEGkg1qHyZBG8G7d1X6aGDcFKo4B4WcLpbBiAkHoBebeZAB7BtL7oMnDDIQuqAoLHw9SPOUuNgmynhSaLhSg1RyaZBi9AkpoWQgpjl2PTopMgZAw0jSzLtfV62fdn" ^
"https://graph.facebook.com/v2.6/act_958472597545817/ads"
:: Ad id: 6051508860225, Creative id: 6051509172025

:: Get Ad Preview
curl -G ^
  -d 'ad_format=DESKTOP_FEED_STANDARD' ^
  -d 'access_token=CAAGjcNa36u0BAAyZCkAAIrEuaUZAsYio9xki94nsFAka71ySOE9CJR1fOMEGkg1qHyZBG8G7d1X6aGDcFKo4B4WcLpbBiAkHoBebeZAB7BtL7oMnDDIQuqAoLHw9SPOUuNgmynhSaLhSg1RyaZBi9AkpoWQgpjl2PTopMgZAw0jSzLtfV62fdn' ^
  https://graph.facebook.com/v2.6/6051509172025/previews