from selenium.webdriver.chrome.options import Options
from selenium import webdriver
# Selenium Timing Check
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.by import By
from selenium.common.exceptions import TimeoutException
#tqdm
from tqdm import tqdm

import sys
import re
import subprocess

if len(sys.argv) != 2:
    print("Correct usage : moe-dl <link>")
    sys.exit()
elif len(sys.argv) == 2:
    if str(sys.argv[1]).startswith("https://twist.moe/a/"):
        opt = Options()
        opt.add_argument("--headless")
        driver = webdriver.Chrome(options=opt)
        driver.get(str(sys.argv[1]))
        n = None
        try:
            call = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.TAG_NAME, "title")))
            n = call.get_attribute("text")
        except TimeoutException:
            print("Timeout while loading page info.")

        n = re.sub('(.\(.+)|(.Episode.+)', "", n)
        c = len(driver.find_elements_by_class_name("episode-number"))
        print("Found Anime : ", n)
        print("Episodes : ", str(c))
        vx = None
        try:
            call = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.TAG_NAME, "video")))
            vx = call.get_attribute("src")
        except TimeoutException:
            print("Timeout while loading page info.")
        check = re.search('\/\d+$', str(sys.argv[1]))
        link = ""
        if check is not None:
            link = re.sub('\/\d+$', "/", str(sys.argv[1]))
        else:
            link = str(sys.argv[1])
        last = ("%s%s" % (link, str(c)))
        driver.quit()
        driver2 = webdriver.Chrome(options=opt)
        driver2.get(last)
        vy = None
        try:
            call = WebDriverWait(driver2, 10).until(
                EC.presence_of_element_located((By.TAG_NAME, "video")))
            vy = call.get_attribute("src")
        except TimeoutException:
            print("Timeout while loading page info.")
        driver2.quit()
        diff = [i for i in range(len(vx)) if vx[i] != vy[i]]
        diffx = ""
        diffy = ""
        for i in diff:
            diffx += vx[i]
            diffy += vy[i]
        if int(diffx, 10) == 1 and int(diffy, 10) == c:
            print("[Decoded] formatting logic.")
            print("Downloading with aria2c will start soon.")
            for i in tqdm(range(1, c+1)):
                if i < 10:
                    dllink = vx[:diff[0]] + ('0%d' % i) + vx[diff[1]+1:]
                    cmd = [
                        'aria2c', '-o', ("%s MOEDL %d - somedesc-%d%s" % (n, i, i, dllink[-4:])),'--continue=true', dllink]
                    
                    p = subprocess.Popen(cmd, stdout=subprocess.PIPE)
                    p.wait()
                else:
                    dllink = vx[:diff[0]] + str(i) + vx[diff[1]+1:]
                    cmd = [
                        'aria2c', '-o', ("%s MOEDL %d - somedesc-%d%s" % (n, i, i, dllink[-4:])),'--continue=true' ,dllink]
                    
                    p = subprocess.Popen(cmd, stdout=subprocess.PIPE)
                    p.wait()
        else:
            print(
                "formatting logic could not be decoded. please report it to roridev on gh.")

        sys.exit()

    else:
        print("Not a valid [TWIST.MOE] link.")
        print("Valid link example : https://twist.moe/a/imouto-sae-ireba-ii/")
        sys.exit()