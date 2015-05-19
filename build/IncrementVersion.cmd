set /p Increment=Really increment the revision number for Imb? Enter Y to proceed: 
if %Increment%==Y tools\setcsprojver .. /i:fv,r /env:imbversion

pause