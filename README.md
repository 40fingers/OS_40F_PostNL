# OS_40F_PostNL
## Openstore PostNL integration by [40Fingers](https://www.40ffingers.net/)

This [OpenStore](https://github.com/openstore-ecommerce/OpenStore) integration for PostNL will create shipping labels with PostNL when an order is payed and/or shipped.
The label is stored with the order and sent by mail to a configurable address.

### The settings are quite self explanatory:
![image](https://user-images.githubusercontent.com/4275042/150740092-ec25349d-0063-46d5-bcf9-07e1f28e2dc2.png)


### The plugin
the plugin will self-install like this:
```
<genxml>
	<hidden>
		<index datatype="double">23</index>
	</hidden>
	<textbox>
		<ctrl update="save">os_40f_postnl</ctrl>
		<name update="save">PostNL Integration</name>
		<icon update="save"/>
		<group update="save">Admin</group>
		<path update="save">/DesktopModules/NBright/OS_40F_PostNL/Settings.ascx</path>
		<help update="save"/>
		<description update="save"/>
	</textbox>
	<checkbox>
		<hidden update="save">False</hidden>
	</checkbox>
	<dropdownlist/>
	<checkboxlist>
		<securityroles update="save">
			<chk value="True" data="Administrators">Administrators</chk>
			<chk value="False" data="Manager">Manager</chk>
			<chk value="False" data="Editor">Editor</chk>
			<chk value="False" data="ClientEditor">ClientEditor</chk>
			<chk value="False" data="Sales">Sales</chk>
		</securityroles>
	</checkboxlist>
	<radiobuttonlist/>
	<interfaces>
		<genxml>
			<files/>
			<hidden/>
			<textbox>
				<assembly>OS_40F_PostNL</assembly>
				<namespaceclass>OS_40F_PostNL.EventProvider</namespaceclass>
			</textbox>
			<checkbox>
				<active>True</active>
				<default>False</default>
			</checkbox>
			<dropdownlist>
				<providertype>06</providertype>
			</dropdownlist>
			<checkboxlist/>
			<radiobuttonlist/>
		</genxml>
		<genxml>
			<files/>
			<hidden/>
			<textbox>
				<assembly>OS_40F_PostNL</assembly>
				<namespaceclass>OS_40F_PostNL.AjaxProvider</namespaceclass>
			</textbox>
			<checkbox>
				<active>True</active>
				<default>False</default>
			</checkbox>
			<dropdownlist>
				<providertype>12</providertype>
			</dropdownlist>
			<checkboxlist/>
			<radiobuttonlist/>
		</genxml>
	</interfaces>
</genxml>

```
