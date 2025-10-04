/system script
add comment="\D1\EA\F0\E8\EF\F2 \F1\EC\E5\ED\FB \CF\C0\D0\CE\CB\DF \E8 SSID \
    \E4\EB\FF WiFi" dont-require-permissions=no name="UpdateWi-Fi " owner=\
    root policy=\
    ftp,reboot,read,write,policy,test,password,sniff,sensitive,romon source="#\
    \_====================================================================\r\
    \n# \D1\EA\F0\E8\EF\F2 \F1\EC\E5\ED\FB \CF\C0\D0\CE\CB\DF (\E4\E8\ED\E0\EC\
    \E8\F7\E5\F1\EA\E0\FF \E4\EB\E8\ED\E0) \E8 SSID \F1 \E0\E2\F2\EE-\EE\EF\F0\
    \E5\E4\E5\EB\E5\ED\E8\E5\EC\r\
    \n# ====================================================================\r\
    \n\r\
    \n# --- \CD\C0\D1\D2\D0\CE\C9\CA\C8 ---\r\
    \n\r\
    \n# --- \CD\E0\F1\F2\F0\EE\E9\EA\E8 \EF\E0\F0\EE\EB\FF ---\r\
    \n# \C7\E0\E4\E0\E9\F2\E5 \E4\E8\E0\EF\E0\E7\EE\ED \E4\EB\E8\ED\FB \EF\E0\
    \F0\EE\EB\FF (\EE\F2 8 \E4\EE 63 \F1\E8\EC\E2\EE\EB\EE\E2)\r\
    \n:local minPasswordLength 8\r\
    \n:local maxPasswordLength 10\r\
    \n\r\
    \n# --- \CD\E0\F1\F2\F0\EE\E9\EA\E8 SSID ---\r\
    \n# true - \EC\E5\ED\FF\F2\FC SSID, false - \EC\E5\ED\FF\F2\FC \F2\EE\EB\
    \FC\EA\EE \EF\E0\F0\EE\EB\FC\r\
    \n:local changeSsid false\r\
    \n# \C4\EB\E8\ED\E0 \E3\E5\ED\E5\F0\E8\F0\F3\E5\EC\EE\E3\EE SSID (\E5\F1\
    \EB\E8 \F1\EC\E5\ED\E0 \E2\EA\EB\FE\F7\E5\ED\E0)\r\
    \n:local ssidLength 6\r\
    \n\r\
    \n\r\
    \n# --- \CF\E5\F0\E5\EC\E5\ED\ED\FB\E5 \E4\EB\FF \E0\E2\F2\EE-\EE\EF\F0\E5\
    \E4\E5\EB\E5\ED\E8\FF ---\r\
    \n:local securityProfile \"\"\r\
    \n:local wlan24Interface \"\"\r\
    \n\r\
    \n# --- \CF\CE\C8\D1\CA \C0\CA\D2\C8\C2\CD\CE\C3\CE \C8\CD\D2\C5\D0\D4\C5\
    \C9\D1\C0 2.4\C3\C3\F6 \C8 \C5\C3\CE \CF\D0\CE\D4\C8\CB\DF ---\r\
    \n:foreach i in=[/interface wireless find where disabled=no] do={\r\
    \n    :if ([:len \$wlan24Interface] = 0) do={\r\
    \n        :local currentBand [/interface wireless get \$i band]\r\
    \n        :if ([:find \$currentBand \"2ghz\"] >= 0) do={\r\
    \n            :set wlan24Interface [/interface wireless get \$i name]\r\
    \n            :set securityProfile [/interface wireless get \$i security-p\
    rofile]\r\
    \n        }\r\
    \n    }\r\
    \n}\r\
    \n\r\
    \n# --- \CF\D0\CE\C2\C5\D0\CA\C0: \CD\C0\C9\C4\C5\CD \CB\C8 \C8\CD\D2\C5\
    \D0\D4\C5\C9\D1 \C8 \CF\D0\CE\D4\C8\CB\DC ---\r\
    \n:if ([:len \$wlan24Interface] = 0) do={\r\
    \n    :log error \"\CD\E5 \F3\E4\E0\EB\EE\F1\FC \ED\E0\E9\F2\E8 \E0\EA\F2\
    \E8\E2\ED\FB\E9 2.4\C3\C3\F6 \E8\ED\F2\E5\F0\F4\E5\E9\F1. \D1\EC\E5\ED\E0 \
    \EE\F2\EC\E5\ED\E5\ED\E0.\"\r\
    \n    :put \"\CE\D8\C8\C1\CA\C0: \C0\EA\F2\E8\E2\ED\FB\E9 2.4\C3\C3\F6 \E8\
    \ED\F2\E5\F0\F4\E5\E9\F1 \ED\E5 \ED\E0\E9\E4\E5\ED!\"\r\
    \n} else={\r\
    \n    :log info \"\C0\E2\F2\EE\EC\E0\F2\E8\F7\E5\F1\EA\E8 \EE\EF\F0\E5\E4\
    \E5\EB\E5\ED \E8\ED\F2\E5\F0\F4\E5\E9\F1 '\$wlan24Interface' \E8 \E5\E3\EE\
    \_\EF\F0\EE\F4\E8\EB\FC '\$securityProfile'.\"\r\
    \n\r\
    \n    # --- \CF\CE\C4\C3\CE\D2\CE\C2\CA\C0 \CA \C3\C5\CD\C5\D0\C0\D6\C8\C8\
    \_---\r\
    \n    :local timeStr [/system clock get time]\r\
    \n    :local uptimeStr [:tostr [/system resource get uptime]]\r\
    \n    :local cpuLoad [/system resource get cpu-load]\r\
    \n    :local timeHours [:tonum [:pick \$timeStr 0 2]]\r\
    \n    :local timeMinutes [:tonum [:pick \$timeStr 3 5]]\r\
    \n    :local timeSeconds [:tonum [:pick \$timeStr 6 8]]\r\
    \n    :local baseSeed ((\$timeHours * 3600) + (\$timeMinutes * 60) + \$tim\
    eSeconds + (\$cpuLoad * 1000))\r\
    \n    :local uptimeNum 0\r\
    \n    :for j from=0 to=([:len \$uptimeStr] - 1) do={\r\
    \n        :local char [:pick \$uptimeStr \$j]\r\
    \n        :local asciiCode [:tonum \$char]\r\
    \n        :if ([:typeof \$asciiCode] = \"num\") do={ :set uptimeNum (\$upt\
    imeNum + \$asciiCode) }\r\
    \n    }\r\
    \n    :set baseSeed (\$baseSeed + \$uptimeNum)\r\
    \n\r\
    \n    # --- \C2\C0\CB\C8\C4\C0\D6\C8\DF \C8 \CE\CF\D0\C5\C4\C5\CB\C5\CD\C8\
    \C5 \C4\CB\C8\CD\DB \CF\C0\D0\CE\CB\DF ---\r\
    \n    # \C7\E0\F9\E8\F2\E0 \EE\F2 \ED\E5\EA\EE\F0\F0\E5\EA\F2\ED\FB\F5 \E7\
    \ED\E0\F7\E5\ED\E8\E9\r\
    \n    :if (\$minPasswordLength < 8) do={ :set minPasswordLength 8 }\r\
    \n    :if (\$maxPasswordLength > 63) do={ :set maxPasswordLength 63 }\r\
    \n    :if (\$minPasswordLength > \$maxPasswordLength) do={ :set minPasswor\
    dLength \$maxPasswordLength }\r\
    \n    \r\
    \n    # \D0\E0\F1\F7\E5\F2 \F1\EB\F3\F7\E0\E9\ED\EE\E9 \E4\EB\E8\ED\FB \E2\
    \_\E7\E0\E4\E0\ED\ED\EE\EC \E4\E8\E0\EF\E0\E7\EE\ED\E5\r\
    \n    :local lengthRange (\$maxPasswordLength - \$minPasswordLength + 1)\r\
    \n    :local randomLengthOffset ((\$baseSeed) % \$lengthRange)\r\
    \n    :local actualPasswordLength (\$minPasswordLength + \$randomLengthOff\
    set)\r\
    \n\r\
    \n    # --- \C3\C5\CD\C5\D0\C0\D6\C8\DF \D1\CB\D3\D7\C0\C9\CD\CE\C3\CE \CF\
    \C0\D0\CE\CB\DF ---\r\
    \n    :local newPassword \"\"\r\
    \n    # \C4\EE\E1\E0\E2\EB\E5\ED\FB \F1\E8\EC\E2\EE\EB\FB \F2\E8\F0\E5 \E8\
    \_\ED\E8\E6\ED\E5\E3\EE \EF\EE\E4\F7\E5\F0\EA\E8\E2\E0\ED\E8\FF\r\
    \n    :local passChars \"abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ\
    23456789-_\"\r\
    \n    :local passCharsLen [:len \$passChars]\r\
    \n    :local currentSeed \$baseSeed\r\
    \n    # \C8\F1\EF\EE\EB\FC\E7\F3\E5\EC \E4\E8\ED\E0\EC\E8\F7\E5\F1\EA\E8 \
    \F1\E3\E5\ED\E5\F0\E8\F0\EE\E2\E0\ED\ED\F3\FE \E4\EB\E8\ED\F3\r\
    \n    :for i from=1 to=\$actualPasswordLength do={\r\
    \n        :set currentSeed (((\$currentSeed * 1103515245) + 12345) % 21474\
    83648)\r\
    \n        :local mixedSeed (\$currentSeed + (\$i * 104729))\r\
    \n        :local charIndex (\$mixedSeed % \$passCharsLen)\r\
    \n        :if (\$charIndex < 0) do={ :set charIndex (\$charIndex * -1) }\r\
    \n        :set newPassword (\$newPassword . [:pick \$passChars \$charIndex\
    \_(\$charIndex + 1)])\r\
    \n    }\r\
    \n    :log info (\"\D1\E3\E5\ED\E5\F0\E8\F0\EE\E2\E0\ED \ED\EE\E2\FB\E9 \
    \EF\E0\F0\EE\EB\FC (\E4\EB\E8\ED\E0: \$actualPasswordLength): \" . \$newPa\
    ssword)\r\
    \n\r\
    \n    # --- \C3\C5\CD\C5\D0\C0\D6\C8\DF \CD\CE\C2\CE\C3\CE SSID (\C5\D1\CB\
    \C8 \C2\CA\CB\DE\D7\C5\CD\CE) ---\r\
    \n    :local newSsid \"\"\r\
    \n    :if (\$changeSsid) do={\r\
    \n        :local ssidChars \"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRS\
    TUVWXYZ\"\r\
    \n        :local ssidCharsLen [:len \$ssidChars]\r\
    \n        :set currentSeed (\$baseSeed + 1)\r\
    \n        :for i from=1 to=\$ssidLength do={\r\
    \n            :set currentSeed (((\$currentSeed * 1664525) + 1013904223) %\
    \_2147483648)\r\
    \n            :local charIndex (\$currentSeed % \$ssidCharsLen)\r\
    \n            :if (\$charIndex < 0) do={ :set charIndex (\$charIndex * -1)\
    \_}\r\
    \n            :set newSsid (\$newSsid . [:pick \$ssidChars \$charIndex (\$\
    charIndex + 1)])\r\
    \n        }\r\
    \n        :log info (\"\D1\E3\E5\ED\E5\F0\E8\F0\EE\E2\E0\ED \ED\EE\E2\FB\
    \E9 SSID: \" . \$newSsid)\r\
    \n    }\r\
    \n\r\
    \n    # --- \CF\D0\C8\CC\C5\CD\C5\CD\C8\C5 \C8\C7\CC\C5\CD\C5\CD\C8\C9 ---\
    \r\
    \n    :do {\r\
    \n        :local oldPassword [/interface wireless security-profiles get [f\
    ind name=\$securityProfile] wpa2-pre-shared-key]\r\
    \n        :local oldSsid [/interface wireless get [find name=\$wlan24Inter\
    face] ssid]\r\
    \n        \r\
    \n        /interface wireless security-profiles set [find name=\$securityP\
    rofile] authentication-types=wpa2-psk wpa2-pre-shared-key=\$newPassword\r\
    \n        :log info (\"\CF\E0\F0\EE\EB\FC \E4\EB\FF \EF\F0\EE\F4\E8\EB\FF \
    '\$securityProfile' \E8\E7\EC\E5\ED\B8\ED.\")\r\
    \n\r\
    \n        :if (\$changeSsid) do={\r\
    \n            /interface wireless set [find name=\$wlan24Interface] ssid=\
    \$newSsid\r\
    \n            :log info (\"SSID \E4\EB\FF \E8\ED\F2\E5\F0\F4\E5\E9\F1\E0 '\
    \$wlan24Interface' \E8\E7\EC\E5\ED\B8\ED.\")\r\
    \n        }\r\
    \n        \r\
    \n        # --- \C2\DB\C2\CE\C4 \D0\C5\C7\D3\CB\DC\D2\C0\D2\CE\C2 \C2 \CA\
    \CE\CD\D1\CE\CB\DC ---\r\
    \n        :put \"================================\"\r\
    \n        :put (\"\C8\ED\F2\E5\F0\F4\E5\E9\F1: \" . \$wlan24Interface)\r\
    \n        :put (\"\CF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\EE\F1\F2\
    \E8: \" . \$securityProfile)\r\
    \n        \r\
    \n        :if (\$changeSsid) do={\r\
    \n             :put (\"\D1\D2\C0\D0\DB\C9 SSID: \" . \$oldSsid)\r\
    \n             :put (\"\CD\CE\C2\DB\C9 SSID: \" . \$newSsid)\r\
    \n        } else={\r\
    \n             :put (\"SSID \ED\E5 \EC\E5\ED\FF\EB\F1\FF: \" . \$oldSsid)\
    \r\
    \n        }\r\
    \n\r\
    \n        :put (\"\D1\D2\C0\D0\DB\C9 \CF\C0\D0\CE\CB\DC: \" . \$oldPasswor\
    d)\r\
    \n        # \C4\EE\E1\E0\E2\EB\FF\E5\EC \E2\FB\E2\EE\E4 \F4\E0\EA\F2\E8\F7\
    \E5\F1\EA\EE\E9 \E4\EB\E8\ED\FB \EF\E0\F0\EE\EB\FF\r\
    \n        :put (\"\CD\CE\C2\DB\C9 \CF\C0\D0\CE\CB\DC: \" . \$newPassword .\
    \_\" (\E4\EB\E8\ED\E0: \" . \$actualPasswordLength . \")\")\r\
    \n        :put \"================================\"\r\
    \n\r\
    \n    } on-error={\r\
    \n        :log error \"\CE\F8\E8\E1\EA\E0 \EF\F0\E8 \EF\F0\E8\EC\E5\ED\E5\
    \ED\E8\E8 \E8\E7\EC\E5\ED\E5\ED\E8\E9!\"\r\
    \n        :put \"\CE\D8\C8\C1\CA\C0: \CD\E5 \F3\E4\E0\EB\EE\F1\FC \E8\E7\
    \EC\E5\ED\E8\F2\FC \EF\E0\F0\E0\EC\E5\F2\F0\FB WiFi!\"\r\
    \n    }\r\
    \n}"
/system scheduler
add comment="\C7\E0\EF\F3\F1\EA \F1\EA\F0\E8\EF\F2\E0 \F1\EC\E5\ED\FB \CF\C0\
    \D0\CE\CB\DF \E8 SSID \E4\EB\FF WiFi" interval=1d name=UpdateWi-Fi \
    on-event="/system script run UpdateWi-Fi" policy=\
    ftp,reboot,read,write,policy,test,password,sniff,sensitive,romon \
    start-date=oct/04/2025 start-time=07:00:00
/ip service
set ssh address=192.168.10.102/32 port=222 disabled=no
/user
add address=192.168.10.102/32 group=read name=WiFi_Window password=36LVCZLvSi420lT4
