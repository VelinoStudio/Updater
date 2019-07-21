# Updater
更新器

使用UpdaterUI制作更新配置文件，主要以文件版本为主，MD5为辅。将制作的配置文件与更新的程序下的所有文件原样放入HTTP服务器。
客户端调用 ：
Updater updater = new Updater(HTTP链接, 更新配置文件);
（更新配置文件中记录的是相对路径，HTTP链接应该是程序的根目录，更新配置文件也需要在这个目录中）


updater.CheckUpdate(Form)；
（这个方法唯一的参数是父窗体，使用时可以阻止父窗体被点击到。可以使用单独的线程运行，不会影响到父窗口内代码的运行，而同时又确保不会点击到父窗口。
该方法有一个重载，CheckUpdate<T>（Form），使用这个重载方法可以可以使用自定义的更新窗体，这个自定义的窗体必须继承于"UpdateForm"，"UpdateForm"使用了IUpdateForm接口。继承了"UpdateForm"，可以使用UpdateProgressing事件获取更新过程中的各种事件。）
  
Updater.UpdateProgressing事件与更新窗体中的UpdateProgressing事件功能相同。


================================================================


UpdaterUI用于制作更新的配置文件，通过该程序的配置文件，可以指定哪些文件或哪些目录下的文件（不包含子目录）使用MD5校验的方式更新，方便更新一些不包含文件版本的文件，比如文本类型的配置文件或图片。
配置文件中设置如下，需要的请自行添加，理论上可以有无限多个。但是使用MD5校验方式更新会影响更新的效率，建议不要太多。


<appSettings>
    <add key="MD5Hash_File_1" value="文件1"/>
    <add key="MD5Hash_File_2" value="文件2"/>
    <add key="MD5Hash_File_3" value="bin\文件3"/>
    <add key="MD5Hash_File_x" value="bin\data\文件x"/>
    <add key="MD5Hash_Directory_1" value="bin\data\"/>
    <add key="MD5Hash_Directory_2" value="bin\image\"/>
    <add key="MD5Hash_Directory_3" value="bin\gif\"/>
    <add key="MD5Hash_Directory_x" value="bin\xxxxx\"/>
</appSettings>
