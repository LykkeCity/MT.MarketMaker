![Build status](http://teamcity.lykkex.net/app/rest/builds/aggregated/strob:(buildType:(project:(id:MarginTrading_MarketMaker)))/statusIcon.svg)

# Lykke MarginTrading MarketMaker
This service provides liquidity to Lykke Margin Trading Exchange.

It collects orderbooks from external exchanges connectors (or the Lykke Spot prices sources) and produces a resulting orderbook for each instrument, which are transferred to the Lykke Margin Trading Backend apps (https://github.com/LykkeCity/MT) and used there to create market making limit orders.

Processing includes exchanges fault-tolerance, outlies and errors detection, applying markups, etc.
