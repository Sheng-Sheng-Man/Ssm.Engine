#!/bin/bash

# 文件注册 Linux 安装脚本

user=`whoami`
if [ "$user" != "root" ]; then
    echo "[+] restart with sudo ..."
    sudo $0
    exit 1
fi

# info
proName="ShengShengMan Script Shell"
proFileName="ssm"
proMimeName="ssm"
proDecription="ShengShengMan Script File"
proMimeXmlPath="/usr/share/mime/application-x-$proMimeName.xml"
proDesktopPath="/usr/share/applications/application-x-$proMimeName.desktop"
proFolder=`pwd`
proFilePath="$proFolder/$proFileName"
proIcon="$proFolder/script.png"

echo "[+] Create file $proMimeXmlPath ..."
echo '<?xml version="1.0" encoding="UTF-8"?>' > $proMimeXmlPath
echo '<mime-info xmlns="http://www.freedesktop.org/standards/shared-mime-info">' >> $proMimeXmlPath
echo "<mime-type type=\"application/x-$proMimeName\">" >> $proMimeXmlPath
echo "<comment>$proDecription</comment>" >> $proMimeXmlPath
echo "<icon name=\"application-x-$proMimeName\"/>" >> $proMimeXmlPath
echo "<glob-deleteall/>" >> $proMimeXmlPath
echo "<glob pattern=\"*.$proMimeName\"/>" >> $proMimeXmlPath
echo '</mime-type>' >> $proMimeXmlPath
echo '</mime-info>' >> $proMimeXmlPath

echo "[+] Create file $proDesktopPath ..."
echo '[Desktop Entry]' > $proDesktopPath
echo "Version=1.0" >> $proDesktopPath
echo "Type=Application" >> $proDesktopPath
echo "Name=$proName" >> $proDesktopPath
echo "Icon=\"$proIcon\"" >> $proDesktopPath
echo "Exec=\"$proFilePath\" %f" >> $proDesktopPath
echo "NoDisplay=false" >> $proDesktopPath
echo "Categories=Utility;" >> $proDesktopPath
echo "StartupNotify=false" >> $proDesktopPath
echo "MimeType=application/x-$proMimeName" >> $proDesktopPath
echo "Terminal=true" >> $proDesktopPath

echo "[!] Link $proFilePath ..."
sudo rm -rf /usr/bin/$proMimeName
sudo ln -s $proFilePath /usr/bin/$proMimeName
sudo chmod 777 /usr/bin/$proMimeName

echo "[!] Update Mime ..."
sudo xdg-mime install $proMimeXmlPath
sudo xdg-mime default application-x-$proMimeName.desktop application/x-$proMimeName
sudo update-mime-database /usr/share/mime