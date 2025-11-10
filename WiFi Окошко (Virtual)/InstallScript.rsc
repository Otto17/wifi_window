/system script
add comment="\D1\EA\F0\E8\EF\F2 \F1\EE\E7\E4\E0\ED\E8\FF \E2\E8\F0\F2\F3\E0\
    \EB\FC\ED\EE\E3\EE WiFi" dont-require-permissions=no\
    name=Install_Virtual_Wi-Fi owner=root policy=\
    ftp,reboot,read,write,policy,test,password,sniff,sensitive,romon source="#\
    \_--- \CD\C0\D1\D2\D0\CE\C9\CA\C8 ---\r\
    \n# \CD\E0\E7\E2\E0\ED\E8\E5 \EE\F1\ED\EE\E2\ED\EE\E3\EE \E1\F0\E8\E4\E6\
    \E0\r\
    \n:local mainBridge \"lan\"\r\
    \n\r\
    \n# \C2\EE\E7\EC\EE\E6\ED\FB\E5 \E7\ED\E0\F7\E5\ED\E8\FF:\r\
    \n# \"2ghz\"  \97 \E8\F1\EA\E0\F2\FC 2.4 \C3\C3\F6 \E8\ED\F2\E5\F0\F4\E5\
    \E9\F1\r\
    \n# \"5ghz\"  \97 \E8\F1\EA\E0\F2\FC 5 \C3\C3\F6 \E8\ED\F2\E5\F0\F4\E5\E9\
    \F1\r\
    \n:local targetBand \"2ghz\"\r\
    \n\r\
    \n# \CD\E0\E7\E2\E0\ED\E8\E5 \F1\EE\E7\E4\E0\E2\E0\E5\EC\EE\E3\EE \E2\E8\
    \F0\F2\F3\E0\EB\FC\ED\EE\E3\EE \E8\ED\F2\E5\F0\F4\E5\E9\F1\E0\r\
    \n:local virtualName \"Client_WLAN\"\r\
    \n# SSID \E8 \EF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\EE\F1\F2\E8\r\
    \n:local virtualSsid \"Client_WiFi\"\r\
    \n:local virtualProfile \"security_Client_WLAN\"\r\
    \n\r\
    \n# ====================================================================\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC \C1\F0\E8\E4\E6 \"bridge_Client_WLAN\":\r\
    \n/interface bridge\r\
    \nadd comment=\"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\FB\\E9 WiFi\" name=b\
    ridge_Client_WLAN\r\
    \n\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC \EF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\
    \EE\F1\F2\E8 (WPA2-PSK) \F1 \E1\E0\E7\EE\E2\FB\EC \EF\E0\F0\EE\EB\E5\EC:\r\
    \n/interface wireless security-profiles\r\
    \nadd authentication-types=wpa2-psk comment=\\\r\
    \n    \"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\FB\\E9 WiFi\" mode=dynamic-k\
    eys name=\\\r\
    \n    security_Client_WLAN supplicant-identity=\"\" wpa2-pre-shared-key=12\
    3456789\r\
    \n\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC \E2\E8\F0\F2\F3\E0\EB\FC\ED\FB\E9 WiFi \F1 \E8\
    \E7\EE\EB\FF\F6\E8\E5\E9 \E8 \E0\E2\F2\EE\EC\E0\F2\E8\F7\E5\F1\EA\E8\EC \
    \E2\FB\E1\EE\F0\EE\EC \E4\E8\E0\EF\E0\E7\EE\ED\E0:\r\
    \n:local wlanInterface \"\"\r\
    \n\r\
    \n# --- \CF\EE\E8\F1\EA \E0\EA\F2\E8\E2\ED\EE\E3\EE \E8\ED\F2\E5\F0\F4\E5\
    \E9\F1\E0 \F1 \ED\F3\E6\ED\FB\EC \E4\E8\E0\EF\E0\E7\EE\ED\EE\EC ---\r\
    \n:foreach i in=[/interface wireless find where disabled=no] do={\r\
    \n    :if ([:len \$wlanInterface] = 0) do={\r\
    \n        :local currentBand [/interface wireless get \$i band]\r\
    \n        :if ([:find \$currentBand \$targetBand] >= 0) do={\r\
    \n            :set wlanInterface [/interface wireless get \$i name]\r\
    \n        }\r\
    \n    }\r\
    \n}\r\
    \n\r\
    \n# --- \C5\F1\EB\E8 \ED\E5 \ED\E0\E9\E4\E5\ED \97 \E2\E7\FF\F2\FC \EF\E5\
    \F0\E2\FB\E9 \E0\EA\F2\E8\E2\ED\FB\E9 \E8\ED\F2\E5\F0\F4\E5\E9\F1 ---\r\
    \n:if ([:len \$wlanInterface] = 0) do={\r\
    \n    :set wlanInterface [/interface wireless get ([/interface wireless fi\
    nd where disabled=no]->0) name]\r\
    \n    :log warning (\"\CD\E5 \ED\E0\E9\E4\E5\ED \E8\ED\F2\E5\F0\F4\E5\E9\
    \F1 \E4\E8\E0\EF\E0\E7\EE\ED\E0 \" . \$targetBand . \", \E2\FB\E1\F0\E0\ED\
    \_\EF\E5\F0\E2\FB\E9 \E0\EA\F2\E8\E2\ED\FB\E9: \" . \$wlanInterface)\r\
    \n}\r\
    \n\r\
    \n# --- \CF\F0\EE\E2\E5\F0\EA\E0 \F1\F3\F9\E5\F1\F2\E2\EE\E2\E0\ED\E8\FF \
    \E2\E8\F0\F2\F3\E0\EB\FC\ED\EE\E3\EE \E8\ED\F2\E5\F0\F4\E5\E9\F1\E0 ---\r\
    \n:if ([:len [/interface wireless find name=\$virtualName]] > 0) do={\r\
    \n    :log info (\"\C8\ED\F2\E5\F0\F4\E5\E9\F1 '\" . \$virtualName . \"' \
    \F3\E6\E5 \F1\F3\F9\E5\F1\F2\E2\F3\E5\F2, \EF\F0\EE\EF\F3\F1\EA \F1\EE\E7\
    \E4\E0\ED\E8\FF.\")\r\
    \n    :put (\"\C8\ED\F2\E5\F0\F4\E5\E9\F1 \" . \$virtualName . \" \F3\E6\
    \E5 \F1\F3\F9\E5\F1\F2\E2\F3\E5\F2, \EF\F0\EE\EF\F3\F1\EA.\")\r\
    \n} else={\r\
    \n    /interface wireless add \\\r\
    \n        comment=\"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\FB\\E9 WiFi \\E4\
    \\EB\\FF \\EA\\EB\\E8\\E5\\ED\\\r\
    \n    \\F2\\EE\\E2\" \\\r\
    \n        default-forwarding=no disabled=no keepalive-frames=disabled \\\r\
    \n        master-interface=\$wlanInterface multicast-buffering=disabled \\\
    \r\
    \n        name=\$virtualName security-profile=\$virtualProfile \\\r\
    \n        ssid=\$virtualSsid wps-mode=disabled\r\
    \n\r\
    \n    :log info (\"\C2\E8\F0\F2\F3\E0\EB\FC\ED\FB\E9 \E8\ED\F2\E5\F0\F4\E5\
    \E9\F1 '\" . \$virtualName . \"' \F3\F1\EF\E5\F8\ED\EE \F1\EE\E7\E4\E0\ED \
    (master: \" . \$wlanInterface . \").\")\r\
    \n    :put \"\C3\EE\F2\EE\E2\EE: \F1\EE\E7\E4\E0\ED \$virtualName (master:\
    \_\$wlanInterface, \E4\E8\E0\EF\E0\E7\EE\ED: \$targetBand)\"\r\
    \n}\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC \EF\F3\EB \E0\E4\F0\E5\F1\EE\E2:\r\
    \n/ip pool\r\
    \nadd comment=\"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\FB\\E9 WiFi\" name=p\
    ool_Client_WLAN \\\r\
    \n    ranges=10.0.0.100-10.0.0.254\r\
    \n\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC DHCP \F1\E5\F0\E2\E5\F0:\r\
    \n/ip dhcp-server\r\
    \nadd address-pool=pool_Client_WLAN disabled=no interface=bridge_Client_WL\
    AN \\\r\
    \n    lease-time=5m name=dhcp_Client_WLAN\r\
    \n\r\
    \n\r\
    \n# \C4\EE\E1\E0\E2\EB\FF\E5\EC \E2\E8\F0\F2\F3\E0\EB\FC\ED\FB\E9 WiFi \EA\
    \_\E8\E7\EE\EB\E8\F0\EE\E2\E0\ED\ED\EE\EC\F3 \E1\F0\E8\E4\E6\F3:\r\
    \n/interface bridge port\r\
    \nadd bridge=bridge_Client_WLAN comment=\\\r\
    \n    \"\\C8\\E7\\EE\\EB\\FF\\F6\\E8\\FF \\E2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\
    \\ED\\EE\\E3\\EE WiFi\" \\\r\
    \n    interface=Client_WLAN\r\
    \n\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC IP-\E0\E4\F0\E5\F1:\r\
    \n/ip address\r\
    \nadd address=10.0.0.1/24 comment=\"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\\
    FB\\E9 WiFi\" \\\r\
    \n    interface=bridge_Client_WLAN network=10.0.0.0\r\
    \n\r\
    \n\r\
    \n# \D1\EE\E7\E4\E0\B8\EC DHCP \F1\E5\F2\FC (\F8\EB\FE\E7, DNS, \EC\E0\F1\
    \EA\E0):\r\
    \n/ip dhcp-server network\r\
    \nadd address=10.0.0.0/24 comment=\"\\C2\\E8\\F0\\F2\\F3\\E0\\EB\\FC\\ED\\\
    FB\\E9 WiFi\" \\\r\
    \n    dns-server=10.0.0.1,77.88.8.4 gateway=10.0.0.1 netmask=24\r\
    \n\r\
    \n\r\
    \n# \C7\E0\EF\F0\E5\F9\E0\E5\EC \E4\EE\F1\F2\F3\EF \E8\E7 \E2\E8\F0\F2\F3\
    \E0\EB\FC\ED\EE\E9 \F1\E5\F2\E8 \E2 \EE\F1\ED\EE\E2\ED\F3\FE: \r\
    \n/ip firewall filter\r\
    \nadd place-before=0 action=drop chain=forward comment=\\\r\
    \n    \"\\C1\\EB\\EE\\EA\\E8\\F0\\EE\\E2\\E0\\F2\\FC guest WiFi -> \$mainB\
    ridge\" in-interface=\\\r\
    \n    bridge_Client_WLAN out-interface=\$mainBridge\r\
    \n\r\
    \n# \C7\E0\EF\F0\E5\F9\E0\E5\EC \E4\EE\F1\F2\F3\EF \E8\E7 \E2\E8\F0\F2\F3\
    \E0\EB\FC\ED\EE\E9 \F1\E5\F2\E8 \E2 \EF\F0\E8\E2\E0\F2\ED\FB\E5 \F1\E5\F2\
    \E8 \E7\E0 WAN (192.168.x.x): \r\
    \nadd place-before=0 action=drop chain=forward comment=\"\\C1\\EB\\EE\\EA\
    \\E8\\F0\\EE\\E2\\E0\\F2\\FC guest\\\r\
    \n    \\_WiFi -> \\EF\\F0\\E8\\E2\\E0\\F2\\ED\\FB\\E5 \\F1\\E5\\F2\\E8 \\E\
    7\\E0 WAN (192.168.x.x\\\r\
    \n    )\" dst-address=192.168.0.0/16 in-interface=bridge_Client_WLAN \\\r\
    \n    out-interface=ether1\r\
    \n\r\
    \n\r\
    \n# \C2\EA\EB\FE\F7\E0\E5\EC NAT (masquerade) \E4\EB\FF \E2\E8\F0\F2\F3\E0\
    \EB\FC\ED\EE\E9 \F1\E5\F2\E8:\r\
    \n/ip firewall nat\r\
    \nadd action=masquerade chain=srcnat comment=\"\\C4\\E0\\B8\\EC \\E4\\EE\\\
    F1\\F2\\F3\\EF \\\r\
    \n    \\E2 Internet \\E4\\EB\\FF \\E2\\F1\\E5\\E9 \\EF\\EE\\E4\\F1\\E5\\F2\
    \\E8\" src-address=\\\r\
    \n    10.0.0.0/24\r\
    \n\r\
    \n# --- \D1\E0\EC\EE\F3\E4\E0\EB\E5\ED\E8\E5 \F1\EA\F0\E8\EF\F2\E0 \EF\EE\
    \F1\EB\E5 \E5\E3\EE \E2\FB\EF\EE\EB\ED\E5\ED\E8\FF ---\r\
    \n:delay 1s\r\
    \n/system script remove Install_Virtual_Wi-Fi\r\
    \n:log info \"\D1\EA\F0\E8\EF\F2 \F3\F1\F2\E0\ED\EE\E2\EA\E8 \E2\E8\F0\F2\
    \F3\E0\EB\FC\ED\EE\E3\EE WiFi \F3\E4\E0\EB\B8\ED\""

add comment="\D1\EA\F0\E8\EF\F2 \E4\E8\ED\E0\EC\E8\F7\E5\F1\EA\EE\E9 \F1\EC\
    \E5\ED\FB \EF\E0\F0\EE\EB\FF \E8 SSID \ED\E0 \E2\E8\F0\F2\F3\E0\EB\FC\ED\
    \EE\EC WiFi" dont-require-permissions=no name=UpdateVirtual_Wi-Fi\
    owner=root policy=\
    ftp,reboot,read,write,policy,test,password,sniff,sensitive,romon source="#\
    \_====================================================================\r\
    \n# \D1\EC\E5\ED\E0 \CF\C0\D0\CE\CB\DF (\E4\E8\ED\E0\EC\E8\F7\E5\F1\EA\E0\
    \FF \E4\EB\E8\ED\E0) \E8, SSID (\EE\EF\F6\E8\EE\ED\E0\EB\FC\ED\EE) \E4\EB\
    \FF \E2\E8\F0\F2\F3\E0\EB\FC\ED\EE\E3\EE Wi\?Fi \E8\ED\F2\E5\F0\F4\E5\E9\
    \F1\E0 \"Client_WLAN\".\r\
    \n# \D2\E5\F1\F2\E8\F0\EE\E2\E0\EB \ED\E0 \F0\EE\F3\F2\E5\F0\E0\F5 : RB951\
    Ui-2nD (RouterOS 6.48); RB951Ui-2HnD (RouterOS 6.49); RBD52G-5HacD2HnD (Ro\
    uterOS 7.20).\r\
    \n# ====================================================================\r\
    \n\r\
    \n# --- \CD\C0\D1\D2\D0\CE\C9\CA\C8 ---\r\
    \n\r\
    \n# \C4\E8\E0\EF\E0\E7\EE\ED \E4\EB\E8\ED\FB \EF\E0\F0\EE\EB\FF (\EE\F2 8 \
    \E4\EE 63 \F1\E8\EC\E2\EE\EB\EE\E2)\r\
    \n:local minPasswordLength 8\r\
    \n:local maxPasswordLength 10\r\
    \n\r\
    \n# true - \EC\E5\ED\FF\F2\FC \E8 SSID, false - \EC\E5\ED\FF\F2\FC \F2\EE\
    \EB\FC\EA\EE \EF\E0\F0\EE\EB\FC\r\
    \n:local changeSsid false\r\
    \n# \C4\EB\E8\ED\E0 \E3\E5\ED\E5\F0\E8\F0\F3\E5\EC\EE\E3\EE SSID (\E5\F1\
    \EB\E8 \F1\EC\E5\ED\E0 \E2\EA\EB\FE\F7\E5\ED\E0)\r\
    \n:local ssidLength 6\r\
    \n\r\
    \n# \D4\E8\EA\F1\E8\F0\EE\E2\E0\ED\ED\EE\E5 \E8\EC\FF \E2\E8\F0\F2\F3\E0\
    \EB\FC\ED\EE\E3\EE \E8\ED\F2\E5\F0\F4\E5\E9\F1\E0\r\
    \n:local wlanInterfaceName \"Client_WLAN\"\r\
    \n\r\
    \n\r\
    \n# --- \CF\D0\CE\C2\C5\D0\CA\C8 \C8 \CE\CF\D0\C5\C4\C5\CB\C5\CD\C8\C5 \CF\
    \D0\CE\D4\C8\CB\DF ---\r\
    \n\r\
    \n# \CD\E0\E9\E4\B8\EC \E8\ED\F2\E5\F0\F4\E5\E9\F1 \EF\EE \E8\EC\E5\ED\E8\
    \r\
    \n:local wlanId [/interface wireless find where name=\$wlanInterfaceName]\
    \r\
    \n:if ([:len \$wlanId] = 0) do={\r\
    \n    :log error (\"\C8\ED\F2\E5\F0\F4\E5\E9\F1 '\" . \$wlanInterfaceName \
    . \"' \ED\E5 \ED\E0\E9\E4\E5\ED. \D1\EC\E5\ED\E0 \EE\F2\EC\E5\ED\E5\ED\E0.\
    \")\r\
    \n    :put (\"\CE\D8\C8\C1\CA\C0: \C8\ED\F2\E5\F0\F4\E5\E9\F1 '\" . \$wlan\
    InterfaceName . \"' \ED\E5 \ED\E0\E9\E4\E5\ED!\")\r\
    \n} else={\r\
    \n\r\
    \n    # \CE\EF\F0\E5\E4\E5\EB\E8\EC \EF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\
    \E0\F1\ED\EE\F1\F2\E8, \EF\F0\E8\E2\FF\E7\E0\ED\ED\FB\E9 \EA \FD\F2\EE\EC\
    \F3 \E8\ED\F2\E5\F0\F4\E5\E9\F1\F3\r\
    \n    :local securityProfile [/interface wireless get \$wlanId security-pr\
    ofile]\r\
    \n    :if ([:len \$securityProfile] = 0) do={\r\
    \n        :log error (\"\C4\EB\FF \E8\ED\F2\E5\F0\F4\E5\E9\F1\E0 '\" . \$w\
    lanInterfaceName . \"' \ED\E5 \EE\EF\F0\E5\E4\E5\EB\B8\ED \EF\F0\EE\F4\E8\
    \EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\EE\F1\F2\E8. \D1\EC\E5\ED\E0 \EE\F2\EC\E5\
    \ED\E5\ED\E0.\")\r\
    \n        :put (\"\CE\D8\C8\C1\CA\C0: \CD\E5 \F3\E4\E0\EB\EE\F1\FC \EE\EF\
    \F0\E5\E4\E5\EB\E8\F2\FC \EF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\EE\
    \F1\F2\E8 \E4\EB\FF '\" . \$wlanInterfaceName . \"'.\")\r\
    \n    } else={\r\
    \n\r\
    \n        :log info (\"\D0\E0\E1\EE\F2\E0\E5\EC \F1 \E2\E8\F0\F2\F3\E0\EB\
    \FC\ED\FB\EC Wi\?Fi '\" . \$wlanInterfaceName . \"', \EF\F0\EE\F4\E8\EB\FC\
    \_'\" . \$securityProfile . \"'.\")\r\
    \n\r\
    \n        # --- \CF\CE\C4\C3\CE\D2\CE\C2\CA\C0 \CA \C3\C5\CD\C5\D0\C0\D6\
    \C8\C8 ---\r\
    \n        :local timeStr [/system clock get time]\r\
    \n        :local uptimeStr [:tostr [/system resource get uptime]]\r\
    \n        :local cpuLoad [/system resource get cpu-load]\r\
    \n        :local timeHours [:tonum [:pick \$timeStr 0 2]]\r\
    \n        :local timeMinutes [:tonum [:pick \$timeStr 3 5]]\r\
    \n        :local timeSeconds [:tonum [:pick \$timeStr 6 8]]\r\
    \n        :local baseSeed ((\$timeHours * 3600) + (\$timeMinutes * 60) + \
    \$timeSeconds + (\$cpuLoad * 1000))\r\
    \n        :local uptimeNum 0\r\
    \n        :for j from=0 to=([:len \$uptimeStr] - 1) do={\r\
    \n            :local char [:pick \$uptimeStr \$j]\r\
    \n            :local asciiCode [:tonum \$char]\r\
    \n            :if ([:typeof \$asciiCode] = \"num\") do={ :set uptimeNum (\
    \$uptimeNum + \$asciiCode) }\r\
    \n        }\r\
    \n        :set baseSeed (\$baseSeed + \$uptimeNum)\r\
    \n\r\
    \n        # --- \C2\C0\CB\C8\C4\C0\D6\C8\DF \C8 \CE\CF\D0\C5\C4\C5\CB\C5\
    \CD\C8\C5 \C4\CB\C8\CD\DB \CF\C0\D0\CE\CB\DF ---\r\
    \n        :if (\$minPasswordLength < 8) do={ :set minPasswordLength 8 }\r\
    \n        :if (\$maxPasswordLength > 63) do={ :set maxPasswordLength 63 }\
    \r\
    \n        :if (\$minPasswordLength > \$maxPasswordLength) do={ :set minPas\
    swordLength \$maxPasswordLength }\r\
    \n\r\
    \n        :if (\$ssidLength < 1)  do={ :set ssidLength 1 }\r\
    \n        :if (\$ssidLength > 32) do={ :set ssidLength 32 }\r\
    \n\r\
    \n        # --- \C4\CB\C8\CD\C0 \CF\C0\D0\CE\CB\DF ---\r\
    \n        :local lengthRange (\$maxPasswordLength - \$minPasswordLength + \
    1)\r\
    \n        :local randomLengthOffset (\$baseSeed % \$lengthRange)\r\
    \n        :local actualPasswordLength (\$minPasswordLength + \$randomLengt\
    hOffset)\r\
    \n\r\
    \n        # --- \C3\C5\CD\C5\D0\C0\D6\C8\DF \D1\CB\D3\D7\C0\C9\CD\CE\C3\CE\
    \_\CF\C0\D0\CE\CB\DF ---\r\
    \n        :local newPassword \"\"\r\
    \n        :local passChars \"abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUV\
    WXYZ23456789-_\"\r\
    \n        :local passCharsLen [:len \$passChars]\r\
    \n        :local currentSeed \$baseSeed\r\
    \n        :for i from=1 to=\$actualPasswordLength do={\r\
    \n            :set currentSeed (((\$currentSeed * 1103515245) + 12345) % 2\
    147483648)\r\
    \n            :local mixedSeed (\$currentSeed + (\$i * 104729))\r\
    \n            :local charIndex (\$mixedSeed % \$passCharsLen)\r\
    \n            :if (\$charIndex < 0) do={ :set charIndex (\$charIndex * -1)\
    \_}\r\
    \n            :set newPassword (\$newPassword . [:pick \$passChars \$charI\
    ndex (\$charIndex + 1)])\r\
    \n        }\r\
    \n        :log info (\"\D1\E3\E5\ED\E5\F0\E8\F0\EE\E2\E0\ED \ED\EE\E2\FB\
    \E9 \EF\E0\F0\EE\EB\FC (\E4\EB\E8\ED\E0: \$actualPasswordLength): \" . \$n\
    ewPassword)\r\
    \n\r\
    \n        # --- \C3\C5\CD\C5\D0\C0\D6\C8\DF \CD\CE\C2\CE\C3\CE SSID (\C5\
    \D1\CB\C8 \C2\CA\CB\DE\D7\C5\CD\CE) ---\r\
    \n        :local newSsid \"\"\r\
    \n        :if (\$changeSsid) do={\r\
    \n            :local ssidChars \"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNO\
    PQRSTUVWXYZ\"\r\
    \n            :local ssidCharsLen [:len \$ssidChars]\r\
    \n            :set currentSeed (\$baseSeed + 1)\r\
    \n            :for i from=1 to=\$ssidLength do={\r\
    \n                :set currentSeed (((\$currentSeed * 1664525) + 101390422\
    3) % 2147483648)\r\
    \n                :local charIndex (\$currentSeed % \$ssidCharsLen)\r\
    \n                :if (\$charIndex < 0) do={ :set charIndex (\$charIndex *\
    \_-1) }\r\
    \n                :set newSsid (\$newSsid . [:pick \$ssidChars \$charIndex\
    \_(\$charIndex + 1)])\r\
    \n            }\r\
    \n            :log info (\"\D1\E3\E5\ED\E5\F0\E8\F0\EE\E2\E0\ED \ED\EE\E2\
    \FB\E9 SSID: \" . \$newSsid)\r\
    \n        }\r\
    \n\r\
    \n        # --- \CF\D0\C8\CC\C5\CD\C5\CD\C8\C5 \C8\C7\CC\C5\CD\C5\CD\C8\C9\
    \_---\r\
    \n        :do {\r\
    \n            :local oldPassword [/interface wireless security-profiles ge\
    t [find name=\$securityProfile] wpa2-pre-shared-key]\r\
    \n            :local oldSsid [/interface wireless get \$wlanId ssid]\r\
    \n\r\
    \n            /interface wireless security-profiles set [find name=\$secur\
    ityProfile] authentication-types=wpa2-psk wpa2-pre-shared-key=\$newPasswor\
    d\r\
    \n            :log info (\"\CF\E0\F0\EE\EB\FC \E4\EB\FF \EF\F0\EE\F4\E8\EB\
    \FF '\$securityProfile' \E8\E7\EC\E5\ED\B8\ED.\")\r\
    \n\r\
    \n            :if (\$changeSsid) do={\r\
    \n                /interface wireless set \$wlanId ssid=\$newSsid\r\
    \n                :log info (\"SSID \E4\EB\FF \E8\ED\F2\E5\F0\F4\E5\E9\F1\
    \E0 '\$wlanInterfaceName' \E8\E7\EC\E5\ED\B8\ED.\")\r\
    \n            }\r\
    \n\r\
    \n            # --- \C2\DB\C2\CE\C4 \D0\C5\C7\D3\CB\DC\D2\C0\D2\CE\C2 \C2 \
    \CA\CE\CD\D1\CE\CB\DC ---\r\
    \n            :put \"================================\"\r\
    \n            :put (\"\C8\ED\F2\E5\F0\F4\E5\E9\F1: \" . \$wlanInterfaceNam\
    e)\r\
    \n            :put (\"\CF\F0\EE\F4\E8\EB\FC \E1\E5\E7\EE\EF\E0\F1\ED\EE\F1\
    \F2\E8: \" . \$securityProfile)\r\
    \n            :if (\$changeSsid) do={\r\
    \n                 :put (\"\D1\D2\C0\D0\DB\C9 SSID: \" . \$oldSsid)\r\
    \n                 :put (\"\CD\CE\C2\DB\C9 SSID: \" . \$newSsid)\r\
    \n            } else={\r\
    \n                 :put (\"SSID \ED\E5 \EC\E5\ED\FF\EB\F1\FF: \" . \$oldSs\
    id)\r\
    \n            }\r\
    \n            :put (\"\D1\D2\C0\D0\DB\C9 \CF\C0\D0\CE\CB\DC: \" . \$oldPas\
    sword)\r\
    \n            :put (\"\CD\CE\C2\DB\C9 \CF\C0\D0\CE\CB\DC: \" . \$newPasswo\
    rd . \" (\E4\EB\E8\ED\E0: \" . \$actualPasswordLength . \")\")\r\
    \n            :put \"================================\"\r\
    \n\r\
    \n        } on-error={\r\
    \n            :log error \"\CE\F8\E8\E1\EA\E0 \EF\F0\E8 \EF\F0\E8\EC\E5\ED\
    \E5\ED\E8\E8 \E8\E7\EC\E5\ED\E5\ED\E8\E9!\"\r\
    \n            :put \"\CE\D8\C8\C1\CA\C0: \CD\E5 \F3\E4\E0\EB\EE\F1\FC \E8\
    \E7\EC\E5\ED\E8\F2\FC \EF\E0\F0\E0\EC\E5\F2\F0\FB \E3\EE\F1\F2\E5\E2\EE\E3\
    \EE Wi\?Fi!\"\r\
    \n        }\r\
    \n    }\r\
    \n}"
/system scheduler
add comment="\C7\E0\EF\F3\F1\EA \F1\EA\F0\E8\EF\F2\E0 \F1\EC\E5\ED\FB \CF\C0\
    \D0\CE\CB\DF \E8 SSID \E4\EB\FF WiFi" interval=1d name=UpdateVirtual_Wi-Fi \
    on-event="/system script run UpdateVirtual_Wi-Fi" policy=\
    ftp,reboot,read,write,policy,test,password,sniff,sensitive,romon \
    start-date=oct/04/2025 start-time=07:00:00
/ip service
set ssh address=192.168.10.102/32 port=222 disabled=no
/user
add address=192.168.10.102/32 group=read name=WiFi_Virtual_Window password=12345678
/system script run Install_Virtual_Wi-Fi
